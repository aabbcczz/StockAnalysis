using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using CommandLine;
using StockAnalysis.Share;
using TradingStrategy;
using TradingStrategy.Strategy;
using TradingStrategy.Base;
using TradingStrategyEvaluation;
using System.Threading;
using CsvHelper;

namespace PredicatorCmdClient
{
    static class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(with => { with.HelpWriter = Console.Error; });

            var parseResult = parser.ParseArguments<Options>(args);

            if (parseResult.Errors.Any())
            {
                var helpText = CommandLine.Text.HelpText.AutoBuild(parseResult);
                Console.WriteLine("{0}", helpText);

                Environment.Exit(-2);
            }

            var options = parseResult.Value;

            options.BoundaryCheck();
            options.Print(Console.Out);

            Run(options);
#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
#endif
        }

        static void ErrorExit(string message, int exitCode = -1)
        {
            Console.Error.WriteLine(message);
            Environment.Exit(exitCode);
        }

        static void CheckOptions(Options options)
        {
            if (options.InitialCapital <= 0.0)
            {
                ErrorExit("Initial capital must be greater than 0.0");
            }

            if (string.IsNullOrWhiteSpace(options.StockDataSettingsFile))
            {
                ErrorExit("Stock data settings file is empty string");
            }

            if (string.IsNullOrWhiteSpace(options.CombinedStrategySettingsFile))
            {
                ErrorExit("Combined strategy settings file is empty string");
            }

            if (string.IsNullOrWhiteSpace(options.SymbolFile))
            {
                ErrorExit("Symbol file is empty string");
            }
        }

        static string AddPrefixToFileName(string fileName, string prefix)
        {
            string trueFileName = prefix + Path.GetFileName(fileName);
            return Path.Combine(Path.GetDirectoryName(fileName), trueFileName);
        }

        static IEnumerable<string> LoadSymbolOfStocks(string symbolFile)
        {
            var symbols = File.ReadAllLines(symbolFile).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            return symbols;
        }

        static StockBlockRelationshipManager LoadStockBlockRelationship(string relationshipFile)
        {
            var relationships = StockBlockRelationship.LoadFromFile(relationshipFile);

            return new StockBlockRelationshipManager(relationships);
        }

        static IEnumerable<Position> LoadPositions(string positionFile)
        {
            if (string.IsNullOrEmpty(positionFile))
            {
                return new List<Position>();
            }

            using (var reader = new StreamReader(positionFile, Encoding.UTF8))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    return csvReader.GetRecords<Position>().ToList();
                }
            }
        }

        static void SavePositions(string positionFile, IEnumerable<Position> positions)
        {
            using (var writer = new StreamWriter(
                positionFile,
                false,
                Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(positions);
                }
            }
        }

        static IEnumerable<string> SelectStockAndFilterBlocks(ref StockBlockRelationshipManager manager, int minStockPerBlock)
        {
            var stocks = manager.FindMinimumStockSetCoveredAllBlocks(minStockPerBlock);
            manager = manager.CreateSubsetForStocks(stocks);

            return stocks;
        }

        static void Run(Options options)
        {
            // check the validation of options
            CheckOptions(options);

            // load settings from files
            var combinedStrategySettings = CombinedStrategySettings.LoadFromFile(options.CombinedStrategySettingsFile);
            var stockDataSettings = ChinaStockDataSettings.LoadFromFile(options.StockDataSettingsFile);
            var positions = LoadPositions(options.PositionFile);

            // load symbols and stock name table
            var stockNameTable = new TradingObjectNameTable<StockName>(stockDataSettings.StockNameTableFile);
            var symbols = LoadSymbolOfStocks(options.SymbolFile);

            // load stock block relationship if necessary, and filter symbols
            StockBlockRelationshipManager stockBlockRelationshipManager = null;
            if (!string.IsNullOrWhiteSpace(options.StockBlockRelationshipFile))
            {
                stockBlockRelationshipManager = LoadStockBlockRelationship(options.StockBlockRelationshipFile);

                // filter stock block relationship for loaded symbols only
                stockBlockRelationshipManager = stockBlockRelationshipManager.CreateSubsetForStocks(symbols);

                // symbols will be updated according to stock-block relationships
                symbols = stockBlockRelationshipManager.Stocks;
            }

            var allDataFiles = symbols
                .Select(stockDataSettings.BuildActualDataFilePathAndName)
                .ToArray();

            // initialize data provider
            var dataProvider
                = new ChinaStockDataProvider(
                    stockNameTable,
                    allDataFiles,
                    options.StartDate,
                    options.EndDate,
                    options.WarmupPeriods);

            var finalSymbols = dataProvider.GetAllTradingObjects().Select(to => to.Symbol);
            var filteredStockBlockRelationshipManager = stockBlockRelationshipManager == null
                ? null
                : stockBlockRelationshipManager.CreateSubsetForStocks(finalSymbols);

            if (filteredStockBlockRelationshipManager != null)
            {
                var filteredSymbols = filteredStockBlockRelationshipManager.Stocks;

                var filteredDataFiles = filteredSymbols
                    .Select(stockDataSettings.BuildActualDataFilePathAndName)
                    .ToArray();

                // rebuild data provider according to filtered symbols
                dataProvider = new ChinaStockDataProvider(
                    stockNameTable,
                    filteredDataFiles,
                    options.StartDate, 
                    options.EndDate,
                    options.WarmupPeriods);
            }

            // initialize combined strategy assembler
            var combinedStrategyAssembler = new CombinedStrategyAssembler(combinedStrategySettings, false);

            var strategyInstances
                = new List<Tuple<CombinedStrategy, IDictionary<Tuple<int, ParameterAttribute>, object>>>();

            IDictionary<Tuple<int, ParameterAttribute>, object> values;
            while ((values = combinedStrategyAssembler.GetNextSetOfParameterValues()) != null)
            {
                var strategy = combinedStrategyAssembler.NewStrategy();

                strategyInstances.Add(Tuple.Create(strategy, values));
            }

            if (strategyInstances.Count != 1)
            {
                throw new InvalidDataException("Strategy has more or less than one instance, please check strategy settings");
            }

            string predictionContextDirectory = options.PredicationName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
            if (!Directory.Exists(predictionContextDirectory))
            {
                Directory.CreateDirectory(predictionContextDirectory);
            }

            using (PredicationContext context = new PredicationContext(predictionContextDirectory))
            {
                PredicateStrategy(
                    context,
                    strategyInstances.First().Item1,
                    strategyInstances.First().Item2,
                    options.StartDate,
                    options.InitialCapital,
                    options.CurrentCapital,
                    positions,
                    dataProvider,
                    filteredStockBlockRelationshipManager,
                    options.PositionFrozenDays);
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
        }

        private static void PredicateStrategy(
            PredicationContext context,
            ITradingStrategy strategy,
            IDictionary<Tuple<int, ParameterAttribute>, object> parameterValues,
            DateTime startDate,
            double initialCapital,
            double currentCapital,
            IEnumerable<Position> activePositions,
            ITradingDataProvider dataProvider,
            StockBlockRelationshipManager relationshipManager,
            int positionFrozenDays)
        {
            var predicator = new TradingStrategyPredicator(
                        initialCapital,
                        currentCapital,
                        strategy,
                        parameterValues,
                        dataProvider,
                        relationshipManager,
                        positionFrozenDays,
                        activePositions,
                        context.Logger);

            try
            {
                predicator.Predicate();

                var auxiliaryData = predicator.PredicatedTransactions
                    .Select(
                        transaction =>
                        {
                            var symbolIndex = dataProvider.GetIndexOfTradingObject(transaction.Symbol);
                            Bar lastBar;
                            
                            if (!dataProvider.GetLastEffectiveBar(symbolIndex, transaction.SubmissionTime, out lastBar))
                            {
                                lastBar.OpenPrice = 0.0;
                                lastBar.HighestPrice = 0.0;
                                lastBar.LowestPrice = 0.0;
                                lastBar.ClosePrice = 0.0;
                            }

                            return new AuxiliaryData()
                            {
                                Symbol = transaction.Symbol,
                                Name = transaction.Name,
                                OpenPrice = lastBar.OpenPrice,
                                ClosePrice = lastBar.ClosePrice,
                                HighestPrice = lastBar.HighestPrice,
                                LowestPrice = lastBar.LowestPrice
                            };
                        });
                    
                context.SaveResults(
                    dataProvider,
                    parameterValues, 
                    predicator.ActivePositions, 
                    predicator.PredicatedTransactions,
                    auxiliaryData);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("{0}", ex);
                return;
            }
        }
    }
}
