using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using CommandLine;
using StockAnalysis.Share;
using TradingStrategy;
using TradingStrategyEvaluation;

using System.Threading;
using CsvHelper;

namespace DataStatistic
{
    static class Program
    {
        private static bool _toBeStopped = false;

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
            if (string.IsNullOrWhiteSpace(options.StockDataSettingsFile))
            {
                ErrorExit("Stock data settings file is empty string");
            }

            if (string.IsNullOrWhiteSpace(options.CodeFile))
            {
                ErrorExit("Code file is empty string");
            }

            if (string.IsNullOrWhiteSpace(options.OutputFile))
            {
                ErrorExit("Output file is empty string");
            }
        }

        static IEnumerable<string> LoadCodeOfStocks(string codeFile)
        {
            var codes = File.ReadAllLines(codeFile)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => StockName.GetCanonicalCode(s))
                .OrderBy(s => s)
                .ToArray();

            return codes;
        }

        static void Run(Options options)
        {
            // check the validation of options
            CheckOptions(options);

            // register handler for Ctrl+C/Ctrl+Break
            Console.CancelKeyPress += ConsoleCancelKeyPress;

            // load settings from files
            var stockDataSettings = ChinaStockDataSettings.LoadFromFile(options.StockDataSettingsFile);

            // load codes and stock name table
            var stockNameTable = new StockNameTable(stockDataSettings.StockNameTableFile);
            var codes = LoadCodeOfStocks(options.CodeFile);

            var allDataFiles = codes
                .Select(stockDataSettings.BuildActualDataFilePathAndName)
                .ToArray();

            // initialize data provider
            var dataProvider
                = new ChinaStockDataProvider(
                    stockNameTable,
                    allDataFiles,
                    options.StartDate,
                    options.EndDate,
                    0);

            try
            {
                var tradingObjects = dataProvider.GetAllTradingObjects();
                var barCounters = CreateBarCounters().ToArray();

                if (barCounters != null && barCounters.Count() > 0)
                {
                    Parallel.For(
                        0, 
                        tradingObjects.Length,
                        // below line is for performance profiling only.
                        new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, 
                        t =>
                        {
                            if (_toBeStopped)
                            {
                                return;
                            }

                            var tradingObject = tradingObjects[t];

                            var bars = dataProvider.GetAllBarsForTradingObject(tradingObject.Index);

                            foreach (var counter in barCounters)
                            {
                                counter.Count(bars, tradingObject);
                            }                            
                        });

                    foreach(var counter in barCounters)
                    {
                        string outputFile = counter.Name + "." + options.OutputFile;

                        counter.SaveResults(outputFile);
                    }
                }
            }
            catch
            {
                _toBeStopped = true;
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
        }

        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _toBeStopped = true;

            e.Cancel = false;
        }

        private static IEnumerable<IBarCounter> CreateBarCounters()
        {
            //yield return new PossibilityOfRaisingHighCounter();
            yield return new GapDownBounceCounter(10, 0.0, 0.0, 0.0);
        }
    }
}
