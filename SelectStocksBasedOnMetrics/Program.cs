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

            ProcessListOfFiles(options.InputFileList, outputFile, options.KeptRecord);

            Console.WriteLine("Done.");

            return 0;
        }

        static IEnumerable<StockMetricRecord> ProcessOneFile(string file, int keptRecord)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            if (keptRecord <= 0)
            {
                return null;
            }

            Csv inputData = Csv.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            List<StockMetricRecord> data = new List<StockMetricRecord>(keptRecord);

            var trimmedInputData = inputData.Rows.Skip(Math.Max(0, inputData.RowCount - keptRecord));
            string[] metricNames = inputData.Header.Skip(2).ToArray();

            return trimmedInputData.Select(
                row => 
                    {
                        StockMetricRecord record = new StockMetricRecord();

                        record.Code = row[0];
                        record.Date = DateTime.Parse(row[1]);
                        record.MetricNames = metricNames;
                        record.Metrics = new double[row.Length - 2];

                        for (int i = 2; i < row.Length; ++i)
                        {
                            record.Metrics[i - 2] = double.Parse(row[i]);
                        }

                        return record;
                    });
        }

        static void ProcessListOfFiles(string listFile, string outputFile, int keptRecord)
        {
            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentNullException();
            }

            // Get all input files from list file
            string[] files = File.ReadAllLines(listFile, Encoding.UTF8);

            List<StockMetricRecord> records = new List<StockMetricRecord>();

            using (StreamWriter outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                Parallel.ForEach(
                    files,
                    (string file) =>
                    {
                        if (String.IsNullOrWhiteSpace(file))
                        {
                            return;
                        }

                        StockMetricRecord[] metrics = ProcessOneFile(file.Trim(), keptRecord).Reverse().ToArray();

                        StockMetricRecord expandedMetric = new StockMetricRecord();
                        expandedMetric.Code = metrics[0].Code;
                        expandedMetric.Date = metrics[0].Date;
                        expandedMetric.MetricNames = Enumerable
                            .Range(0, metrics.Length)
                            .SelectMany(i => metrics[i].MetricNames
                                .Select(s => "T" + (i == 0 ? "0" : (-i).ToString()) + s))
                            .ToArray();
                        expandedMetric.Metrics = Enumerable
                            .Range(0, metrics.Length)
                            .SelectMany(i => metrics[i].Metrics)
                            .ToArray();

                        lock(records)
                        {
                            records.Add(expandedMetric);
                        }

                        Console.Write(".");
                    });


                outputter.WriteLine("Code,Date,{0}", string.Join(",", records.First().MetricNames));

                foreach (var record in records)
                {
                    outputter.WriteLine(
                        "N{0},{1:yyyy/MM/dd},{2}",
                        record.Code,
                        record.Date,
                        string.Join(",", record.Metrics.Select(v => string.Format("{0:0.00}", v)).ToArray()));
                }

            }
        }
    }
}
