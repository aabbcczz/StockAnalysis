using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using StockAnalysis.Share;
using TradingStrategy;
using TradingStrategy.Strategy;
using TradingStrategyEvaluation;
using System.Threading;

namespace EvaluatorCmdClient
{
    static class Program
    {
        private static EvaluationResultContextManager _contextManager;
        private static long _totalNumberOfStrategies = 0;
        private static long _numberOfEvaluatedStrategies = 0;

        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => Environment.Exit(-2)))
            {
                options.BoundaryCheck();
                options.Print(Console.Out);

                Run(options);
            }

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

        static IEnumerable<Tuple<DateTime, DateTime>> GenerateIntervals(DateTime startDate, DateTime endDate, int yearInterval)
        {
            if (yearInterval <= 0)
            {
                yearInterval = 1000; // a big enough interval
            }

            DateTime actualEndDate;
            do
            {
                actualEndDate = startDate.AddYears(yearInterval).AddDays(-1);
                if (actualEndDate >= endDate)
                {
                    actualEndDate = endDate;
                }

                yield return Tuple.Create(startDate, actualEndDate);

                startDate = startDate.AddYears(1);
                if (startDate >= endDate)
                {
                    yield break;
                }
            } while (actualEndDate < endDate);
        }

        static void SetTotalStrategyNumber(long number)
        {
            Interlocked.CompareExchange(ref _totalNumberOfStrategies, number, 0);
        }

        static void IncreaseProgress()
        {
            long numberOfEvaluatedStrategies 
                = Interlocked.Increment(ref _numberOfEvaluatedStrategies);

            long totalNumberOfEvaluatedStrategies 
                = Interlocked.CompareExchange(ref _totalNumberOfStrategies, 0, 0);

            Console.Write(
                "\r{0}/{1} ({2}%)",
                numberOfEvaluatedStrategies,
                totalNumberOfEvaluatedStrategies,
                (long)numberOfEvaluatedStrategies * 100 / totalNumberOfEvaluatedStrategies);
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

            // register handler for Ctrl+C/Ctrl+Break
            Console.CancelKeyPress += ConsoleCancelKeyPress;

            // load settings from files
            var tradingSettings = TradingSettings.LoadFromFile(options.TradingSettingsFile);
            var combinedStrategySettings = CombinedStrategySettings.LoadFromFile(options.CombinedStrategySettingsFile);
            var stockDataSettings = ChinaStockDataSettings.LoadFromFile(options.StockDataSettingsFile);

            // load codes and stock name table
            var stockNameTable = new StockNameTable(stockDataSettings.StockNameTableFile);
            var codes = LoadCodeOfStocks(options.CodeFile);
            var dataFiles = codes
                .Select(stockDataSettings.BuildActualDataFilePathAndName)
                .ToArray();

            // generate evaluation time intervals
            var intervals = 
                GenerateIntervals(
                    options.StartDate, 
                    options.EndDate, 
                    options.YearInterval)
                .ToArray();

            using (_contextManager = new EvaluationResultContextManager(options.EvaluationName))
            {
                // save evluation summary
                var evaluationSummary = new EvaluationSummary
                {
                    StrategySettings = combinedStrategySettings.GetActiveSettings(),
                    TradingSettings = tradingSettings,
                    DataSettings = stockDataSettings,
                    StartTime = options.StartDate,
                    EndTime = options.EndDate,
                    YearInterval = options.YearInterval,
                    ObjectNames = codes
                        .Select(c => stockNameTable.ContainsStock(c)
                            ? c + '|' + stockNameTable[c].Names[0]
                            : c)
                        .ToArray()
                };

                _contextManager.SaveEvaluationSummary(evaluationSummary);

                foreach (var interval in intervals)
                {
                    // initialize data provider
                    var dataProvider
                        = new ChinaStockDataProvider(
                            stockNameTable,
                            dataFiles,
                            interval.Item1, // interval start date
                            interval.Item2, // interval end date
                            options.WarmupPeriods);

                    // initialize combined strategy assembler
                    var combinedStrategyAssembler = new CombinedStrategyAssembler(combinedStrategySettings);

                    var strategyInstances
                        = new List<Tuple<CombinedStrategy, IDictionary<ParameterAttribute, object>>>();

                    IDictionary<ParameterAttribute, object> values;
                    while ((values = combinedStrategyAssembler.GetNextSetOfParameterValues()) != null)
                    {
                        var strategy = combinedStrategyAssembler.NewStrategy();

                        strategyInstances.Add(Tuple.Create(strategy, values));
                    }

                    if (strategyInstances.Any())
                    {
                        // initialize ResultSummary
                        ResultSummary.Initialize(strategyInstances.First().Item2);
                    }

                    SetTotalStrategyNumber(intervals.Count() * strategyInstances.Count());

                    try
                    {
                        Parallel.ForEach(
                            strategyInstances,
                            // below line is for performance profiling only.
                            // new ParallelOptions() { MaxDegreeOfParallelism = 1 }, 
                            t => EvaluateStrategy(
                                    _contextManager,
                                    t.Item1,
                                    t.Item2,
                                    interval.Item1,
                                    interval.Item2,
                                    options.InitialCapital,
                                    dataProvider,
                                    tradingSettings));
                    }
                    finally
                    {
                        lock (_contextManager)
                        {
                            // save result summary
                            _contextManager.SaveResultSummaries();
                        }
                    }
                }
            }

            _contextManager = null;

            Console.WriteLine();
            Console.WriteLine("Done.");
        }

        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_contextManager != null)
            {
                lock (_contextManager)
                {
                    _contextManager.SaveResultSummaries();
                }
            }
        }

        private static void EvaluateStrategy(
            EvaluationResultContextManager contextManager, 
            ITradingStrategy strategy,
            IDictionary<ParameterAttribute, object> parameterValues,
            DateTime startDate,
            DateTime endDate,
            double initialCapital,
            ITradingDataProvider dataProvider,
            TradingSettings tradingSettings)
            //StockNameTable stockNameTable)
        {
            // OutputParameterValues(parameterValues);

            EvaluationResultContext context;

            lock (contextManager)
            {
                context = contextManager.CreateNewContext();
            }

            using (context)
            {
                var evaluator
                    = new TradingStrategyEvaluator(
                        initialCapital,
                        strategy,
                        parameterValues,
                        dataProvider,
                        tradingSettings,
                        context.Logger);

                //EventHandler<EvaluationProgressEventArgs> evaluationProgressHandler =
                //    (object obj, EvaluationProgressEventArgs e) =>
                //    {
                //        Console.Write("\r{0:yyyy-MM-dd} {1}%", e.EvaluationPeriod, (int)(e.EvaluationPercentage * 100.0));
                //    };

                //evaluator.OnEvaluationProgress += evaluationProgressHandler;

                try
                {
                    evaluator.Evaluate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("{0}", ex);
                    return;
                }

                if (!evaluator.Tracker.TransactionHistory.Any())
                {
                    // no transaction
                    return;
                }

                var calculator
                    = new MetricCalculator(
                        //stockNameTable,
                        evaluator.Tracker,
                        dataProvider);

                var metrics = calculator.Calculate();

                // get the overall metric
                var tradeMetrics = metrics as TradeMetric[] ?? metrics.ToArray();
                var overallMetric = tradeMetrics.First(m => m.Code == TradeMetric.CodeForAll);

                // save results
                context.SaveResults(parameterValues, tradeMetrics, evaluator.ClosedPositions);

                // create result summary;
                var resultSummary = new ResultSummary();
                resultSummary.Initialize(
                    context, 
                    parameterValues,
                    startDate,
                    endDate,
                    overallMetric);

                lock (contextManager)
                {
                    contextManager.AddResultSummary(resultSummary);
                }
            }

            IncreaseProgress();
        }
    }
}
