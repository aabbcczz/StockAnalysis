using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Threading;

using StockAnalysis.Share;

namespace GetFinanceReports
{
    class Program
    {
        private const string ServerKey = "serverAddress";

        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => { Environment.Exit(-2); }))
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
            StockNameTable stockNameTable 
                = new StockNameTable
                    (
                        options.StockNameTable, 
                        (string invalidLine) => 
                        {
                            Console.WriteLine("Invalid line: {0}", invalidLine);
                            return true;
                        }
                    );

            // create report fetcher
            IReportFetcher fetcher = ReportFetcherFactory.Create(options.FinanceReportServerAddress);

            // make sure output folder exists, otherwise create it.
            string outputFolder = Path.GetFullPath(options.OutputFolder);
            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    Directory.CreateDirectory(outputFolder);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("create folder {0} failed, error:\n{1}", outputFolder, ex.ToString());
                }
            }

            StockNameTable lastRoundStocks = stockNameTable;
            StockNameTable failedStocks = null;

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
            StockNameTable failedStocks = new StockNameTable();

            string defaultSuffix = fetcher.GetDefaultSuffixOfOutputFile();
            Random rand = new Random();

            foreach (var stock in stocks.StockNames)
            {
                string errorMessage;
                string outputFile = string.Format("{0}.{1}", stock.Code, defaultSuffix);
                outputFile = Path.Combine(outputFolder, outputFile);

                bool succeeded = fetcher.FetchReport(stock, outputFile, out errorMessage);

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
