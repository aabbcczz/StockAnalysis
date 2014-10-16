using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using StockAnalysis.Share;

namespace ProcessDailyStockData
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with => with.HelpWriter = Console.Error);

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
                return -2;
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

            StockNameTable table;

            if (!string.IsNullOrEmpty(options.InputFile))
            {
                // single input file
                StockName name = ProcessOneFile(options.InputFile, options.StartDate, options.EndDate, folder);
                table = new StockNameTable();

                if (name != null)
                {
                    table.AddStock(name);
                }
            }
            else
            {
                table = ProcessListOfFiles(options.InputFileList, options.StartDate, options.EndDate, folder);
            }

            if (!string.IsNullOrEmpty(options.NameFile))
            {
                Console.WriteLine();
                Console.WriteLine("Output name file: {0}", options.NameFile);

                File.WriteAllLines(
                    options.NameFile,
                    table.StockNames.Select(sn => sn.ToString()).ToArray(),
                    Encoding.UTF8);
            }

            Console.WriteLine("Done.");

            return 0;
        }

        static StockName ProcessOneFile(string file, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            var lines = File.ReadAllLines(file, Encoding.GetEncoding("GB2312"));

            // in general the file contains at least 3 lines, 2 lines of header and at least 1 line of data.
            if (lines.Length <= 2)
            {
                Console.WriteLine("Input {0} contains less than 3 lines, ignore it", file);

                return null;
            }

            // first line contains the stock code, name and '日线'
            var fields = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 3)
            {
                Console.WriteLine("Invalid first line in file {0}", file);

                return null;
            }

            var code = fields[0];
            var name = string.Concat(fields.Skip(1).Take(fields.Length - 2).ToArray());

            var stockName = new StockName(code, name);

            var fullDataFile = Path.Combine(outputFileFolder, code + ".day.csv");
            var deltaDataFile = Path.Combine(outputFileFolder, code + ".day.delta.csv");

            var generateDeltaFile = File.Exists(fullDataFile);

            var outputFile = generateDeltaFile ? deltaDataFile : fullDataFile;

            using (var outputter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                var header = "code,date,open,highest,lowest,close,volume,amount";
                const int indexOfVolume = 6;

                outputter.WriteLine(header);

                fields = header.Split(new[] { ',' });
                var expectedFieldCount = fields.Length - 1; // remove the first column 'code' which does not exists in input file

                for (var i = 2; i < lines.Length - 1; ++i)
                {
                    lines[i] = lines[i].Trim();
                    fields = lines[i].Split(new[] { ',' });
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

                        int volume;

                        if (int.TryParse(fields[indexOfVolume], out volume))
                        {
                            if (volume == 0)
                            {
                                continue;
                            }
                        }

                        outputter.WriteLine("{0},{1}", code, lines[i]);
                    }
                }
            }

            if (generateDeltaFile)
            {
                MergeFile(fullDataFile, deltaDataFile);
            }

            return stockName;
        }

        static void MergeFile(string fullDataFile, string deltaDataFile)
        {
            if (!File.Exists(fullDataFile) || !File.Exists(deltaDataFile))
            {
                Console.WriteLine("file {0} or {1} does not exist", fullDataFile, deltaDataFile);
                return;
            }

            var fullData = Csv.Load(fullDataFile, Encoding.UTF8, ",");
            var deltaData = Csv.Load(deltaDataFile, Encoding.UTF8, ",");

            var mergedData = new Csv(fullData.Header);

            var orderedFullData = fullData.Rows.OrderBy(columns => DateTime.Parse(columns[1])).ToArray();
            var orderedDeltaData = deltaData.Rows.OrderBy(columns => DateTime.Parse(columns[1])).ToArray();

            var i = 0;
            var j = 0;

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
                    var date1 = DateTime.Parse(orderedFullData[i][1]);
                    var date2 = DateTime.Parse(orderedDeltaData[j][1]);

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

        static StockNameTable ProcessListOfFiles(string listFile, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            var table = new StockNameTable();

            // Get all input files from list file
            var files = File.ReadAllLines(listFile, Encoding.UTF8);

            Parallel.ForEach(
                files,
                file =>
                {
                    if (!String.IsNullOrWhiteSpace(file))
                    {
                        var stockName = ProcessOneFile(file.Trim(), startDate, endDate, outputFileFolder);

                        if (stockName != null)
                        {
                            lock (table)
                            {
                                table.AddStock(stockName);
                            }
                        }
                    }

                    Console.Write(".");
                });

            return table;
        }
    }
}
