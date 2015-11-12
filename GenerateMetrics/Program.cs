using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using MetricsDefinition;
using StockAnalysis.Share;

namespace GenerateMetrics
{
    static class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => Environment.Exit(-2)))
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

                var returnValue = Run(options);

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

            var folder = Path.GetFullPath(options.OutputFileFolder);

            // try to create output file folder if it does not exist
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Create output file folder {0} failed. Exception: \n{1}", folder, ex);
                    return -3;
                }
            }

            // load metric definitions
            var metrics = LoadMetricsDefinition(options.MetricsDefinitionFile).ToArray();

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



        static void ProcessOneFile(string file, DateTime startDate, DateTime endDate, string outputFileFolder, string[] metrics)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            var data = StockHistoryData.LoadFromFile(file, startDate, endDate);
            if (data == null)
            {
                return;
            }

            var allFieldNames = new List<string>();

            // parse metrics to expression
            var metricExpressions = metrics
                .Select(MetricEvaluationContext.ParseExpression)
                .ToArray();

            // build field names
            for (var i = 0; i < metrics.Length; ++i)
            {
                if (metricExpressions[i].FieldNames.Length == 1)
                {
                    allFieldNames.Add(metrics[i]);
                }
                else
                {
                    var i1 = i;
                    allFieldNames.AddRange(metricExpressions[i].FieldNames.Select(s => metrics[i1] + "." + s));
                }
            }

            // calculate metrics
            var metricValues = data.DataOrderedByTime
                .Select(bar => metricExpressions
                    .SelectMany(
                        m => 
                        { 
                            m.MultipleOutputUpdate(bar); 
                            return m.Values; 
                        })
                    .ToArray())
                .ToList();

            var outputFile = Path.Combine(outputFileFolder, data.Name.Code + ".day.metric.csv");

            using (var outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                var header = "code,date," 
                    + string.Join(",", allFieldNames.Select(MetricHelper.ConvertMetricToCsvCompatibleHead));

                outputter.WriteLine(header);

                var times = data.DataOrderedByTime.Select(d => d.Time).ToArray();

                for (var i = 0; i < times.Length; ++i)
                {
                    var value = string.Join(
                            ",",
                            metricValues[i]
                                .Select(v => string.Format("{0:0.00}", v)));

                    outputter.WriteLine(
                        "{0},{1:yyyy/MM/dd},{2}",
                        data.Name.Code,
                        times[i],
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
            var files = File.ReadAllLines(listFile, Encoding.UTF8);

            Parallel.ForEach(
                files,
                file =>
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
            var lines = File.ReadAllLines(file);

            return lines
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Where(l => !l.StartsWith("#"));
        }
    }
}
