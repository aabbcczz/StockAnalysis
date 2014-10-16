using System;
using System.Configuration;
using System.IO;
using System.Threading;
using CommandLine;
using StockAnalysis.Share;

namespace GetFinanceReports
{
    static class Program
    {
        private const string ServerKey = "serverAddress";

        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => Environment.Exit(-2)))
            {
                options.BoundaryCheck();

                Run(options);
            }
        }

        static void Run(Options options)
        {
            options.FinanceReportServerAddress = ConfigurationManager.AppSettings[ServerKey];

            options.Print(Console.Out);

            // create stock name table
            var stockNameTable 
                = new StockNameTable
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
            StockNameTable failedStocks;

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
                foreach (var stock in failedStocks.StockNames)
                {
                    Console.WriteLine("{0}", stock.Code);
                }
                Console.WriteLine("==============================================");
            }

            Console.WriteLine("Done.");
        }

        private static StockNameTable FetchReports(
            IReportFetcher fetcher, 
            StockNameTable stocks, 
            string outputFolder, 
            int intervalInSecond,
            int randomRangeInSecond)
        {
            var failedStocks = new StockNameTable();

            var defaultSuffix = fetcher.GetDefaultSuffixOfOutputFile();
            var rand = new Random();

            foreach (var stock in stocks.StockNames)
            {
                string errorMessage;
                var outputFile = string.Format("{0}.{1}", stock.Code, defaultSuffix);
                outputFile = Path.Combine(outputFolder, outputFile);

                var succeeded = fetcher.FetchReport(stock, outputFile, out errorMessage);

                if (!succeeded)
                {
                    Console.WriteLine("Fetch report for {0} failed. Error: {1}", stock.Code, errorMessage);
                    failedStocks.AddStock(stock);
                }

                Thread.Sleep((intervalInSecond + rand.Next(randomRangeInSecond)) * 1000);
            }

            return failedStocks;
        }
    }
}
