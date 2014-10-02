using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using StockAnalysis.Share;
using TradingStrategy;
using TradingStrategy.Strategy;
using TradingStrategyEvaluation;

namespace EvaluatorCmdClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => { Environment.Exit(-2); }))
            {
                options.BoundaryCheck();
                options.Print(Console.Out);

                Run(options);
            }
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

            if (string.IsNullOrWhiteSpace(options.TradingSettingsFile))
            {
                ErrorExit("Trading settings file is empty string");
            }

            if (string.IsNullOrWhiteSpace(options.CodeFile))
            {
                ErrorExit("Code file is empty string");
            }
        }

        static void GenerateExampleFiles(Options options)
        {
            var tradingSettings = TradingSettings.GenerateExampleSettings();
            tradingSettings.SaveToFile(options.TradingSettingsFile);

            var combinedStrategySettings = CombinedStrategySettings.GenerateExampleSettings();
            combinedStrategySettings.SaveToFile(options.CombinedStrategySettingsFile);

            var stockDataSettings = ChinaStockDataSettings.GenerateExampleSettings();
            stockDataSettings.SaveToFile(options.StockDataSettingsFile);
        }

        static string[] LoadCodeOfStocks(string codeFile)
        {
            return File.ReadAllLines(codeFile).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        static void Run(Options options)
        {
            // check the validation of options
            CheckOptions(options);

            // generate example files if necessary
            if (options.ShouldGenerateExampleFiles)
            {
                GenerateExampleFiles(options);
                return;
            }

            // load settings from files
            var tradingSettings = TradingSettings.LoadFromFile(options.TradingSettingsFile);
            var combinedStrategySettings = CombinedStrategySettings.LoadFromFile(options.CombinedStrategySettingsFile);
            var stockDataSettings = ChinaStockDataSettings.LoadFromFile(options.StockDataSettingsFile);

            // initialize combined strategy assembler
            var combinedStrategyAssembler = new CombinedStrategyAssembler(combinedStrategySettings);
            
            // load codes and stock name table, and initialize data provider
            StockNameTable stockNameTable = new StockNameTable(stockDataSettings.StockNameTableFile);
            string[] codes = LoadCodeOfStocks(options.CodeFile);
            string[] dataFiles = codes
                .Select(code => stockDataSettings.BuildActualDataFilePathAndName(code))
                .ToArray();

            ChinaStockDataProvider dataProvider 
                = new ChinaStockDataProvider(
                    stockNameTable,
                    dataFiles,
                    options.StartDate,
                    options.EndDate,
                    options.WarmupPeriods);

            using (var contextManager = new EvaluationResultContextManager(options.EvaluationName))
            {
                // evaluate each set of parameter values
                IDictionary<ParameterAttribute, object> parameterValues;
                while ((parameterValues = combinedStrategyAssembler.GetNextSetOfParameterValues()) != null)
                {
                    CombinedStrategy strategy = combinedStrategyAssembler.NewStrategy();

                    EvaluationResultContext context = contextManager.CreateNewContext("s");

                    TradingStrategyEvaluator evaluator
                        = new TradingStrategyEvaluator(
                            options.InitialCapital,
                            strategy,
                            parameterValues,
                            dataProvider,
                            tradingSettings,
                            context.Logger);

                    EventHandler<EvaluationProgressEventArgs> evaluationProgressHandler =
                        (object obj, EvaluationProgressEventArgs e) =>
                        {
                            Console.Write("\r{0:yyyy-MM-dd} {1}", e.EvaluationPeriod, e.EvaluationPercentage);
                        };

                    evaluator.OnEvaluationProgress += evaluationProgressHandler;

                    evaluator.Evaluate();

                    MetricCalculator calculator
                        = new MetricCalculator(
                            stockNameTable,
                            evaluator.History,
                            dataProvider);

                    var metrics = calculator.Calculate();
                    
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Done.");
        }
    }
}
