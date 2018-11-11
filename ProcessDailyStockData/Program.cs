using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using StockAnalysis.Share;

namespace ProcessDailyStockData
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

            IDataProcessor processor = null;

            if (options.IsForFuture)
            {
                processor = new FutureDataProcessor();
            }
            else
            {
                processor = new StockDataProcessor();
            }

            TradingObjectNameTable<TradingObjectName> table;

            if (!string.IsNullOrEmpty(options.InputFile))
            {
                // single input file
                TradingObjectName name = ProcessOneFile(processor, options.InputFile, options.StartDate, options.EndDate, folder);
                table = new TradingObjectNameTable<TradingObjectName>();

                if (name != null)
                {
                    table.AddName(name);
                }
            }
            else
            {
                table = ProcessListOfFiles(processor, options.InputFileList, options.StartDate, options.EndDate, folder);
            }

            if (!string.IsNullOrEmpty(options.NameFile))
            {
                Console.WriteLine();
                Console.WriteLine("Output name file: {0}", options.NameFile);

                File.WriteAllLines(
                    options.NameFile,
                    table.Names.Select(sn => sn.ToString()).ToArray(),
                    Encoding.UTF8);
            }

            if (!string.IsNullOrEmpty(options.SymbolFile))
            {
                Console.WriteLine();
                Console.WriteLine("Output symbol file: {0}", options.SymbolFile);

                File.WriteAllLines(
                    options.SymbolFile,
                    table.Names.Select(sn => sn.Symbol.NormalizedSymbol).ToArray(),
                    Encoding.UTF8);
            }

            Console.WriteLine("Done.");

            return 0;
        }


        static TradingObjectName ProcessOneFile(IDataProcessor processor, string file, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (processor == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            try
            {
                var name = processor.GetName(file);

                if (name == null)
                {
                    return null;
                }

                var fullDataFile = Path.Combine(outputFileFolder, name.Symbol.NormalizedSymbol + ".day.csv");
                var deltaDataFile = Path.Combine(outputFileFolder, name.Symbol.NormalizedSymbol + ".day.delta.csv");

                var generateDeltaFile = File.Exists(fullDataFile);

                var outputFile = generateDeltaFile ? deltaDataFile : fullDataFile;

                processor.ConvertToCsvFile(name, file, outputFile, startDate, endDate);

                if (generateDeltaFile)
                {
                    MergeFile(processor.GetColumnIndexOfDateInCsvFile(), fullDataFile, deltaDataFile);
                }

                return name;
            }
            catch (Exception ex)
            {
                throw new AggregateException(string.Format("failed to process file [{0}]", file), ex);
            }
        }

        static void MergeFile(int dateColumnIndex, string fullDataFile, string deltaDataFile)
        {
            if (!File.Exists(fullDataFile) || !File.Exists(deltaDataFile))
            {
                Console.WriteLine("file {0} or {1} does not exist", fullDataFile, deltaDataFile);
                return;
            }

            var fullData = CsvTable.Load(fullDataFile, Encoding.UTF8, ",");
            var deltaData = CsvTable.Load(deltaDataFile, Encoding.UTF8, ",");

            var mergedData = new CsvTable(fullData.Header);


            var orderedFullData = fullData.Rows
                .Select(columns =>
                    {
                        DateTime date;
                        
                        if (!DateTime.TryParse(columns[dateColumnIndex], out date))
                        {
                            throw new FormatException(string.Format("Failed to parse date {0} in full data file", columns[dateColumnIndex]));
                        }

                        return Tuple.Create(date, columns);
                    })
                .GroupBy(tuple => tuple.Item1)
                .Select(g => g.First())
                .OrderBy(tuple => tuple.Item1)
                .ToArray();

            var orderedDeltaData = deltaData.Rows
                .Select(columns =>
                    {
                        DateTime date;

                        if (!DateTime.TryParse(columns[dateColumnIndex], out date))
                        {
                            throw new FormatException(string.Format("Failed to parse date {0} in delta data file", columns[dateColumnIndex]));
                        }

                        return Tuple.Create(date, columns);
                    })
                .GroupBy(tuple => tuple.Item1)
                .Select(g => g.First())
                .OrderBy(tuple => tuple.Item1)
                .ToArray();

            var i = 0;
            var j = 0;

            while (i < orderedFullData.Length || j < orderedDeltaData.Length)
            {
                if (j >= orderedDeltaData.Length)
                {
                    mergedData.AddRow(orderedFullData[i].Item2);
                    ++i;
                }
                else if (i >= orderedFullData.Length)
                {
                    mergedData.AddRow(orderedDeltaData[j].Item2);
                    ++j;
                }
                else
                {
                    var date1 = orderedFullData[i].Item1;
                    var date2 = orderedDeltaData[j].Item1;

                    if (date1 < date2)
                    {
                        mergedData.AddRow(orderedFullData[i].Item2);
                        ++i;
                    }
                    else if (date1 > date2)
                    {
                        mergedData.AddRow(orderedDeltaData[j].Item2);
                        ++j;
                    }
                    else
                    {
                        mergedData.AddRow(orderedDeltaData[j].Item2);
                        ++i;
                        ++j;
                    }
                }
            }

            // save merged file to full data file
            CsvTable.Save(mergedData, fullDataFile, Encoding.UTF8, ",");

            // remove delta file after merging
            File.Delete(deltaDataFile);
        }

        static TradingObjectNameTable<TradingObjectName> ProcessListOfFiles(IDataProcessor processor, string listFile, DateTime startDate, DateTime endDate, string outputFileFolder)
        {
            if (processor == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFileFolder))
            {
                throw new ArgumentNullException();
            }

            var table = new TradingObjectNameTable<TradingObjectName>();

            // Get all input files from list file
            var files = File.ReadAllLines(listFile, Encoding.UTF8);

            Parallel.ForEach(
                files,
                file =>
                {
                    if (!String.IsNullOrWhiteSpace(file))
                    {
                        var name = ProcessOneFile(processor, file.Trim(), startDate, endDate, outputFileFolder);

                        if (name != null)
                        {
                            lock (table)
                            {
                                table.AddName(name);
                            }
                        }
                    }

                    Console.Write("\r{0}", file);
                });

            Console.WriteLine();

            return table;
        }
    }
}
