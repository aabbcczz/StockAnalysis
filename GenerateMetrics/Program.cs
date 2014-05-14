using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
using StockAnalysis.Share;

namespace GenerateMetrics
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

                if (string.IsNullOrEmpty(options.InputFile) && string.IsNullOrEmpty(options.InputFileList))
                {
                    Console.WriteLine("Neither input file nor input file list is specified");
                    Environment.Exit(-2);
                }

                if (!string.IsNullOrEmpty(options.InputFile) && !string.IsNullOrEmpty(options.InputFileList))
                {
                    Console.WriteLine("Both input file and input file list are specified");
                    Environment.Exit(-2);
                }

                int returnValue = Run(options);

                if (returnValue != 0)
                {
                    Environment.Exit(returnValue);
                }
            }
        }

        static int Run(Options options)
        {
            if (string.IsNullOrEmpty(options.OutputFileFolder))
            {
                Console.WriteLine("output file folder is empty");
            }

            string folder = Path.GetFullPath(options.OutputFileFolder);

            // try to create output file folder if it does not exist
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Create output file folder {0} failed. Exception: \n{1}", folder, ex.ToString());
                    return -3;
                }
            }

            if (!string.IsNullOrEmpty(options.InputFile))
            {
                // single input file
                ProcessOneFile(options.InputFile, options.StartDate, options.EndDate, folder);
            }
            else
            {
                ProcessListOfFiles(options.InputFileList, options.StartDate, options.EndDate, folder);
            }


            Console.WriteLine("Done.");

            return 0;
        }

        static StockDailyHistoryData LoadInputFile(string file, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            Csv inputData = Csv.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            string code = inputData[0][0];
            StockName name = new StockName(code);

            // header is code,date,open,highest,lowest,close,transactionCount,transactionAmountMoney

            List<StockDailySummary> data = new List<StockDailySummary>(inputData.RowCount);

            foreach (var row in inputData.Rows)
            {
                DateTime date = DateTime.Parse(row[1]);
                if (date < startDate || date > endDate)
                {
                    continue;
                }

                StockDailySummary dailyData = new StockDailySummary();

                dailyData.Date = DateTime.Parse(row[1]);
                dailyData.OpenMarketPrice = double.Parse(row[2]);
                dailyData.HighestPrice = double.Parse(row[3]);
                dailyData.LowestPrice = double.Parse(row[4]);
                dailyData.CloseMarketPrice = double.Parse(row[5]);
                dailyData.AmountOfSharesInAllTransaction = double.Parse(row[6]);
                dailyData.AmountOfMoneyInAllTransaction = double.Parse(row[7]);

                if (dailyData.AmountOfMoneyInAllTransaction != 0.0)
                {
                    data.Add(dailyData);
                }
            }

            return new StockDailyHistoryData(name, data);
        }

        static void ProcessOneFile(string file, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            StockDailyHistoryData data = LoadInputFile(file, startDate, endDate);

            var atr20 = new AverageTrueRange(20).Calculate(data.Data).ToArray();
            var stddevAtr20 = new StdDev(20).Calculate(atr20).ToArray();

            var atr40 = new AverageTrueRange(40).Calculate(data.Data).ToArray();
            var stddevAtr40 = new StdDev(40).Calculate(atr40).ToArray();

            var closeMarketPrices = data.Data.Select(s => s.CloseMarketPrice);

            var ma5 = new MovingAverage(5).Calculate(closeMarketPrices).ToArray();
            var ma10 = new MovingAverage(10).Calculate(closeMarketPrices).ToArray();
            var ma20 = new MovingAverage(20).Calculate(closeMarketPrices).ToArray();
            var ma30 = new MovingAverage(30).Calculate(closeMarketPrices).ToArray();
            var ma60 = new MovingAverage(60).Calculate(closeMarketPrices).ToArray();


            string outputFile = Path.Combine(outputFileFolder, data.Name.Code + ".day.metric.csv");

            using (StreamWriter outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                string header = "code,date,close,atr.20,stddev.atr.20,atr.40,stddev.atr.40,ma.5,ma.10,ma.20,ma.30,ma.60";

                outputter.WriteLine(header);

                var summary = data.Data.ToArray();

                for (int i = 0; i < summary.Length; ++i)
                {
                    outputter.WriteLine(
                        "{0},{1:yyyy/MM/dd},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00},{9:0.00},{10:0.00},{11:0.00}",
                        data.Name.Code,
                        summary[i].Date,
                        summary[i].CloseMarketPrice,
                        atr20[i],
                        stddevAtr20[i],
                        atr40[i],
                        stddevAtr40[i],
                        ma5[i],
                        ma10[i],
                        ma20[i],
                        ma30[i],
                        ma60[i]);
                }
            }
        }

        static void ProcessListOfFiles(string listFile, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            // Get all input files from list file
            string[] files = File.ReadAllLines(listFile, Encoding.UTF8);

            foreach (var file in files)
            {
                if (!String.IsNullOrWhiteSpace(file))
                {
                    ProcessOneFile(file.Trim(), startDate, endDate, outputFileFolder);
                }

                Console.Write(".");
            }
        }
    }
}
