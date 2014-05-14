using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace SelectStocksBasedOnMetrics
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

                if (string.IsNullOrEmpty(options.InputFileList))
                {
                    Console.WriteLine("No input file list is specified");
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
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                Console.WriteLine("output file is empty");
            }

            string outputFile = Path.GetFullPath(options.OutputFile);

            ProcessListOfFiles(options.InputFileList, outputFile);

            Console.WriteLine("Done.");

            return 0;
        }

        static IEnumerable<StockDailyMetrics> ProcessOneFile(string file, int keptDays)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            if (keptDays <= 0)
            {
                return null;
            }

            Csv inputData = Csv.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            //"code,date,close,atr.20,stddev.atr.20,atr.40,stddev.atr.40,ma.5,ma.10,ma.20,ma.30,ma.60"

            List<StockDailyMetrics> data = new List<StockDailyMetrics>(keptDays);

            var trimmedInputData = inputData.Rows.Skip(Math.Max(0, inputData.RowCount - keptDays));

            return trimmedInputData.Select(
                row => 
                    {
                        return new StockDailyMetrics
                        {
                            Code = row[0],
                            Date = DateTime.Parse(row[1]),
                            CloseMarketPrice = double.Parse(row[2]),
                            Atr20 = double.Parse(row[3]),
                            StddevAtr20 = double.Parse(row[4]),
                            Atr40 = double.Parse(row[5]),
                            StddevAtr40 = double.Parse(row[6]),
                            Ma5 = double.Parse(row[7]),
                            Ma10 = double.Parse(row[8]),
                            Ma20 = double.Parse(row[9]),
                            Ma30 = double.Parse(row[10]),
                            Ma60 = double.Parse(row[11])
                        };
                    });
        }

        static void ProcessListOfFiles(string listFile, string outputFile)
        {
            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentNullException();
            }

            // Get all input files from list file
            string[] files = File.ReadAllLines(listFile, Encoding.UTF8);

            using (StreamWriter outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                const int keptDays = 5;

                outputter.WriteLine("Code,Date,DayN0Close,DayN0Atr20,DayN0StddevAtr20,DayN0Atr40,DayN0StddevAtr40,DayN0Ma10,DayN0Ma20,DayN1Ma10,DayN1Ma20,DayN2Ma10,DayN2Ma20");

                foreach (var file in files)
                {
                    if (String.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }

                    StockDailyMetrics[] metrics = ProcessOneFile(file.Trim(), keptDays).Reverse().ToArray();

                    StockSelectionMetrics ssm = new StockSelectionMetrics
                    {
                        Code = metrics[0].Code,
                        Date = metrics[0].Date,
                        DayN0CloseMarketPrice = metrics[0].CloseMarketPrice,
                        DayN0Atr20 = metrics[0].Atr20,
                        DayN0Atr40 = metrics[0].Atr40,
                        DayN0StddevAtr20 = metrics[0].StddevAtr20,
                        DayN0StddevAtr40 = metrics[0].StddevAtr40,
                        DayN0Ma10 = metrics[0].Ma10,
                        DayN0Ma20 = metrics[0].Ma20,
                        DayN1Ma10 = metrics[1].Ma10,
                        DayN1Ma20 = metrics[1].Ma20,
                        DayN2Ma10 = metrics[2].Ma10,
                        DayN2Ma20 = metrics[2].Ma20,
                        Predication = StockSelectionMetrics.Movement.NoMove,
                        PredicatedPriceWillCauseMovement = 0.0
                    };

                //    // determine if MA10 is crossing MA20 upwards in recent 3 days
                //    // index 0 is today, 1 is yesterday, and so on.
                //    if (metrics[2].Ma10 < metrics[2].Ma20)
                //    {
                //        if (metrics[0].Ma10 > metrics[0].Ma20)
                //        {
                //            ssm.Predication = StockSelectionMetrics.Movement.UpCross;
                //        }
                //        else
                //        {
                //            if (metrics[0])
                //        }
                //    }
                //    else if (metrics[2].Ma10 > metrics[2].Ma20 && metrics[0].Ma10 < metrics[0].Ma20)
                //    {
                //        ssm.Predication = StockSelectionMetrics.Movement.DownCross;
                //    }

                //    if (metrics[2].Ma10 < metrics[2].Ma20)

                //}

                    outputter.WriteLine(
                        "N{0},{1:yyyy/MM/dd},{2:0.00},{3:0.00},{4:0.00},{5:0.00},{6:0.00},{7:0.00},{8:0.00},{9:0.00},{10:0.00},{11:0.00},{12:0.00}",
                        ssm.Code,
                        ssm.Date,
                        ssm.DayN0CloseMarketPrice,
                        ssm.DayN0Atr20,
                        ssm.DayN0StddevAtr20,
                        ssm.DayN0Atr40,
                        ssm.DayN0StddevAtr40,
                        ssm.DayN0Ma10,
                        ssm.DayN0Ma20,
                        ssm.DayN1Ma10,
                        ssm.DayN1Ma20,
                        ssm.DayN2Ma10,
                        ssm.DayN2Ma20);

                    Console.Write(".");
                }

            }
        }
    }
}
