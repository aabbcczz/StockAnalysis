using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using StockAnalysis.Share;

namespace SelectStocksBasedOnMetrics
{
    static class Program
    {
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

            if (string.IsNullOrEmpty(options.InputFileList))
            {
                Console.WriteLine("No input file list is specified");
                Environment.Exit(-2);
            }

            var returnValue = Run(options);

            if (returnValue != 0)
            {
                Environment.Exit(returnValue);
            }
        }

        static int Run(Options options)
        {
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                Console.WriteLine("output file is empty");
            }

            var outputFile = Path.GetFullPath(options.OutputFile);

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

            var inputData = Csv.Load(file, Encoding.UTF8, ",");

            if (inputData.RowCount == 0)
            {
                return null;
            }

            var trimmedInputData = inputData.Rows.Skip(Math.Max(0, inputData.RowCount - keptRecord));
            var metricNames = inputData.Header.Skip(2).ToArray();

            return trimmedInputData.Select(
                row => 
                    {
                        var record = new StockMetricRecord
                        {
                            Code = row[0],
                            Date = DateTime.Parse(row[1]),
                            MetricNames = metricNames,
                            Metrics = new double[row.Length - 2]
                        };

                        for (var i = 2; i < row.Length; ++i)
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
            var files = File.ReadAllLines(listFile, Encoding.UTF8);

            var records = new List<StockMetricRecord>();

            using (var outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                foreach (var file in files)
                {
                    if (String.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }

                    var rawMetrics = ProcessOneFile(file.Trim(), keptRecord);
                    if (rawMetrics == null)
                    {
                        continue;
                    }

                    var metrics = rawMetrics.Reverse().ToArray();

                    var expandedMetric = new StockMetricRecord
                    {
                        Code = metrics[0].Code,
                        Date = metrics[0].Date,
                        MetricNames = Enumerable
                            .Range(0, metrics.Length)
                            .SelectMany(i => metrics[i].MetricNames
                                .Select(s => "T" + (i == 0 ? "0" : (-i).ToString(CultureInfo.InvariantCulture)) + s))
                            .ToArray(),
                        Metrics = Enumerable
                            .Range(0, metrics.Length)
                            .SelectMany(i => metrics[i].Metrics)
                            .ToArray()
                    };

                    lock (records)
                    {
                        records.Add(expandedMetric);
                    }

                    Console.Write("{0}\r", file);
                }

                Console.WriteLine();

                outputter.WriteLine("Code,Date,{0}", string.Join(",", records.First().MetricNames));

                foreach (var record in records)
                {
                    outputter.WriteLine(
                        "{0},{1:yyyy/MM/dd},{2}",
                        record.Code,
                        record.Date,
                        string.Join(",", record.Metrics.Select(v => string.Format("{0:0.00}", v)).ToArray()));
                }

            }
        }
    }
}
