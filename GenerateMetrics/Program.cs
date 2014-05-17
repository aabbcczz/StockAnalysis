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

            // load metric definitions
            string[] metrics = LoadMetricsDefinition(options.MetricsDefinitionFile).ToArray();

            if (!string.IsNullOrEmpty(options.InputFile))
            {
                // single input file
                ProcessOneFile(options.InputFile, options.StartDate, options.EndDate, folder, metrics);
            }
            else
            {
                ProcessListOfFiles(options.InputFileList, options.StartDate, options.EndDate, folder, metrics);
            }


            Console.WriteLine("Done.");

            return 0;
        }

        static StockHistoryData LoadInputFile(string file, DateTime startDate, DateTime endDate)
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

            List<StockTransactionSummary> data = new List<StockTransactionSummary>(inputData.RowCount);

            foreach (var row in inputData.Rows)
            {
                DateTime date = DateTime.Parse(row[1]);
                if (date < startDate || date > endDate)
                {
                    continue;
                }

                StockTransactionSummary dailyData = new StockTransactionSummary();

                dailyData.Time = DateTime.Parse(row[1]);
                dailyData.OpenPrice = double.Parse(row[2]);
                dailyData.HighestPrice = double.Parse(row[3]);
                dailyData.LowestPrice = double.Parse(row[4]);
                dailyData.ClosePrice = double.Parse(row[5]);
                dailyData.Volume = double.Parse(row[6]);
                dailyData.Amount = double.Parse(row[7]);

                if (dailyData.Volume != 0.0)
                {
                    data.Add(dailyData);
                }
            }

            return new StockHistoryData(name, 86400L, data);
        }

        static void ProcessOneFile(string file, DateTime startDate, DateTime endDate, string outputFileFolder, string[] metrics)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            StockHistoryData data = LoadInputFile(file, startDate, endDate);

            IEnumerable<double>[] input = new IEnumerable<double>[6]
            {
                data.Data.Select(d => d.OpenPrice),
                data.Data.Select(d => d.ClosePrice),
                data.Data.Select(d => d.HighestPrice),
                data.Data.Select(d => d.LowestPrice),
                data.Data.Select(d => d.Volume),
                data.Data.Select(d => d.Amount),
            };

            List<double[]> metricValues = new List<double[]>();
            List<string> allFieldNames = new List<string>();

            Parallel.ForEach(
                metrics,
                m => 
                    {
                        string[] fieldNames;

                        var result = MetricEvaluator.Evaluate(m, input, out fieldNames).Select(r => r.ToArray());

                        lock(metricValues)
                        {
                            metricValues.AddRange(result);

                            if (result.Count() == 1)
                            {
                                allFieldNames.Add(m);
                            }
                            else
                            {
                                allFieldNames.AddRange(fieldNames.Select(s => m + "." + s));
                            }
                        }
                    });

            string outputFile = Path.Combine(outputFileFolder, data.Name.Code + ".day.metric.csv");

            using (StreamWriter outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                string header = "code,date," 
                    + string.Join(",", allFieldNames.Select(m => MetricHelper.ConvertMetricToCsvCompatibleHead(m)));

                outputter.WriteLine(header);

                var summary = data.Data.ToArray();

                for (int i = 0; i < summary.Length; ++i)
                {
                    string value = string.Join(
                            ",",
                            metricValues
                                .Select(v => string.Format("{0:0.00}", v[i])));

                    outputter.WriteLine(
                        "{0},{1:yyyy/MM/dd},{2}",
                        data.Name.Code,
                        summary[i].Time,
                        value);
                }
            }
        }

        static void ProcessListOfFiles(string listFile, DateTime startDate, DateTime endDate, string outputFileFolder, string[] metrics)
        {
            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            // Get all input files from list file
            string[] files = File.ReadAllLines(listFile, Encoding.UTF8);

            Parallel.ForEach(
                files,
                (string file) =>
                {
                    if (!String.IsNullOrWhiteSpace(file))
                    {
                        ProcessOneFile(file.Trim(), startDate, endDate, outputFileFolder, metrics);
                    }

                    Console.Write(".");
                });
        }

        static IEnumerable<string> LoadMetricsDefinition(string file)
        {
            string[] lines = File.ReadAllLines(file);

            return lines
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Where(l => !l.StartsWith("#"));
        }
    }
}
