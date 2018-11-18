using System;
using System.Configuration;
using System.IO;
using System.Threading;
using CommandLine;
using System.Linq;
using StockAnalysis.Common.SymbolName;

namespace GetFinanceReports
{
    static class Program
    {
        private const string ServerKey = "serverAddress";

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
        }

        static void Run(Options options)
        {
            options.FinanceReportServerAddress = ConfigurationManager.AppSettings[ServerKey];

            options.Print(Console.Out);

            // create stock name table
            var stockNameTable
                = TradingObjectNameTable<StockName>.LoadFromFile
                    (
                        options.StockNameTable, 
                        invalidLine => 
                        {
                            Console.WriteLine("Invalid line: {0}", invalidLine);
                            return true;
                        }
                    );

            // create report fetcher
            var fetcher = ReportFetcherFactory.Create(options.FinanceReportServerAddress);

            // make sure output folder exists, otherwise create it.
            var outputFolder = Path.GetFullPath(options.OutputFolder);
            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    Directory.CreateDirectory(outputFolder);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("create folder {0} failed, error:\n{1}", outputFolder, ex);
                }
            }

            var lastRoundStocks = stockNameTable;
            TradingObjectNameTable<StockName> failedStocks;

            while (true)
            {
                failedStocks = FetchReports(fetcher, lastRoundStocks, outputFolder, options.IntervalInSecond, options.RandomRange);

                if (failedStocks == null || failedStocks.Count == 0 || failedStocks.Count == lastRoundStocks.Count)
                {
                    // break when there is no progress.
                    break;
                }

                lastRoundStocks = failedStocks;
            }

            if (failedStocks != null && failedStocks.Count > 0)
            {
                Console.WriteLine("Following stocks' report are not been fetched:");
                Console.WriteLine("==============================================");
                foreach (var name in failedStocks.Names)
                {
                    Console.WriteLine("{0}", name.Symbol.NormalizedSymbol);
                }
                Console.WriteLine("==============================================");
            }

            Console.WriteLine("Done.");
        }

        private static TradingObjectNameTable<StockName> FetchReports(
            IReportFetcher fetcher,
            TradingObjectNameTable<StockName> stockNames, 
            string outputFolder, 
            int intervalInSecond,
            int randomRangeInSecond)
        {
            var failedStocks = new TradingObjectNameTable<StockName>();

            var defaultSuffix = fetcher.GetDefaultSuffixOfOutputFile();
            var rand = new Random();

            foreach (var name in stockNames.Names)
            {
                string errorMessage;
                var outputFile = string.Format("{0}.{1}", name.Symbol.NormalizedSymbol, defaultSuffix);
                outputFile = Path.Combine(outputFolder, outputFile);

                var succeeded = fetcher.FetchReport(name, outputFile, out errorMessage);

                if (!succeeded)
                {
                    Console.WriteLine("Fetch report for {0} failed. Error: {1}", name.Symbol.NormalizedSymbol, errorMessage);
                    failedStocks.AddName(name);
                }

                Thread.Sleep((intervalInSecond + rand.Next(randomRangeInSecond)) * 1000);
            }

            return failedStocks;
        }
    }
}
