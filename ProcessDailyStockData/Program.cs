using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using StockAnalysis.Share;

namespace ProcessDailyStockData
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

        static void ProcessOneFile(string file, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            string[] lines = File.ReadAllLines(file);

            // in general the file contains at least 3 lines, 2 lines of header and at least 1 line of data.
            if (lines.Length <= 2)
            {
                Console.WriteLine("Input {0} contains less than 3 lines, ignore it", file);

                return;
            }

            // first line contains the stock code
            string[] fields = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length == 0)
            {
                Console.WriteLine("Invalid first line in file {0}", file);

                return;
            }

            string code = fields[0];

            string fullDataFile = Path.Combine(outputFileFolder, code + ".day.csv");
            string deltaDataFile = Path.Combine(outputFileFolder, code + ".day.delta.csv");

            bool generateDeltaFile = File.Exists(fullDataFile);

            string outputFile = generateDeltaFile ? deltaDataFile : fullDataFile;

            using (StreamWriter outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                string header = "code,date,open,highest,lowest,close,transactionCount,transactionAmountMoney";

                outputter.WriteLine(header);

                fields = header.Split(new char[] { ',' });
                int expectedFieldCount = fields.Length - 1; // remove the first column 'code' which does not exists in input file

                for (int i = 2; i < lines.Length - 1; ++i)
                {
                    lines[i] = lines[i].Trim();
                    fields = lines[i].Split(new char[] { ',' });
                    if (fields.Length == expectedFieldCount)
                    {
                        // the first field is date
                        DateTime date;

                        if (!DateTime.TryParse(fields[0], out date))
                        {
                            continue;
                        }

                        if (date < startDate || date > endDate)
                        {
                            continue;
                        }

                        outputter.WriteLine("{0},{1}", code, lines[i]);
                    }
                }
            }

            if (generateDeltaFile)
            {
                MergeFile(fullDataFile, deltaDataFile);
            }
        }

        static void MergeFile(string fullDataFile, string deltaDataFile)
        {
            if (!File.Exists(fullDataFile) || !File.Exists(deltaDataFile))
            {
                Console.WriteLine("file {0} or {1} does not exist", fullDataFile, deltaDataFile);
                return;
            }

            Csv fullData = Csv.Load(fullDataFile, Encoding.UTF8, ",");
            Csv deltaData = Csv.Load(deltaDataFile, Encoding.UTF8, ",");

            Csv mergedData = new Csv(fullData.Header);

            var orderedFullData = fullData.Rows.OrderBy(columns => DateTime.Parse(columns[1])).ToArray();
            var orderedDeltaData = deltaData.Rows.OrderBy(columns => DateTime.Parse(columns[1])).ToArray();

            int i = 0;
            int j = 0;

            while (i < orderedFullData.Length || j < orderedDeltaData.Length)
            {
                if (j >= orderedDeltaData.Length)
                {
                    mergedData.AddRow(orderedFullData[i]);
                    ++i;
                }
                else if (i >= orderedFullData.Length)
                {
                    mergedData.AddRow(orderedDeltaData[j]);
                    ++j;
                }
                else
                {
                    DateTime date1 = DateTime.Parse(orderedFullData[i][1]);
                    DateTime date2 = DateTime.Parse(orderedDeltaData[j][1]);

                    if (date1 < date2)
                    {
                        mergedData.AddRow(orderedFullData[i]);
                        ++i;
                    }
                    else if (date1 > date2)
                    {
                        mergedData.AddRow(orderedDeltaData[j]);
                        ++j;
                    }
                    else
                    {
                        mergedData.AddRow(orderedDeltaData[j]);
                        ++i;
                        ++j;
                    }
                }
            }

            // save merged file to full data file
            Csv.Save(mergedData, fullDataFile, Encoding.UTF8, ",");

            // remove delta file after merging
            File.Delete(deltaDataFile);
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
