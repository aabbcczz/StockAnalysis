using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using CommandLine;
using StockAnalysis.Share;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearAlgebra;

namespace CorrelationCalculator
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

            if (string.IsNullOrEmpty(options.InputFileList))
            {
                Console.WriteLine("no input file list is specified");
                Environment.Exit(-2);
            }

            if (string.IsNullOrEmpty(options.OutputFile))
            {
                Console.WriteLine("output file is empty");
                Environment.Exit(-3);
            }

            var returnValue = Run(options);

            if (returnValue != 0)
            {
                Environment.Exit(returnValue);
            }
        }

        static int Run(Options options)
        {
            ProcessListOfFiles(options.InputFileList, options.StartDate, options.EndDate, options.OutputFile, options.Threshold);

            Console.WriteLine("Done.");

            return 0;
        }

        static HistoryData LoadFile(string file, DateTime startDate, DateTime endDate)
        {

            var data = HistoryData.LoadFutureDataFromFile(file, startDate, endDate);

            if (data == null)
            {
                throw new InvalidDataException(string.Format("failed to load data from {0}", file));
            }

            return data;
        }

        static void ProcessListOfFiles(string listFile, DateTime startDate, DateTime endDate, string outputFile, double threshold)
        {
            if (string.IsNullOrEmpty(listFile) || string.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentNullException();
            }

            // Get all input files from list file
            var files = File.ReadAllLines(listFile, Encoding.UTF8);

            List<HistoryData> allData = new List<HistoryData>();

            foreach(var file in files)
            {
                if (!String.IsNullOrWhiteSpace(file))
                {
                    var data = LoadFile(file.Trim(), startDate, endDate);

                    if (data.DataOrderedByTime.Length > 0)
                    {
                        lock (allData)
                        {
                            allData.Add(data);
                        }
                    }
                }

                Console.Write(".");
            }

            // begin to calculate pearson correlation coefficient

            // get names 
            var codeAndNames = allData.Select(d => d.Name.CanonicalCode + " " + d.Name.Names[0]).ToArray();

            // align data and fill missed data
            // 1. select all distince date
            var allDates = allData
                .SelectMany(d => d.DataOrderedByTime.Select(b => b.Time))
                .Distinct()
                .OrderBy(dt => dt)
                .ToArray();

            // 2. normalize data
            var dataSet = allData.Select(d => NormalizeData(d, allDates)).ToArray();

            // we don't call Correlation.PearsonMatrix() directly because we need to align the data.
            int dataSetCount = dataSet.Count();
            var coefficientMatrix = Matrix<double>.Build.Dense(dataSetCount, dataSetCount);
            var intersectionMatrix = new int[dataSetCount, dataSetCount];

            for (int row = 0; row < dataSetCount; ++row)
            {
                for (int column = row; column < dataSetCount; ++column)
                {
                    var intersection = Enumerable.Range(0, allDates.Length)
                        .Where(i => dataSet[row][i] != 0.0 && dataSet[column][i] != 0.0);

                    double correlationCoefficient;
                    int intersectionLength = intersection.Count();

                    if (intersection.Count() == 0)
                    {
                        correlationCoefficient = 0.0;
                    }
                    else
                    {
                        double[] rowSubArray = intersection.Select(i => dataSet[row][i]).ToArray();
                        double[] columnSubArray = intersection.Select(i => dataSet[column][i]).ToArray();

                        correlationCoefficient = Correlation.Pearson(rowSubArray, columnSubArray);
                    }

                    //// find first non-zero data and last non-zero data
                    //int rowStart = Enumerable
                    //    .Range(0, dataSet[row].Count())
                    //    .First(i => dataSet[row][i] != 0.0);

                    //int rowEnd = Enumerable
                    //    .Range(0, dataSet[row].Count())
                    //    .Last(i => dataSet[row][i] != 0.0);

                    //int columnStart = Enumerable
                    //    .Range(0, dataSet[column].Count())
                    //    .First(i => dataSet[column][i] != 0.0);

                    //int columnEnd = Enumerable
                    //    .Range(0, dataSet[column].Count())
                    //    .Last(i => dataSet[column][i] != 0.0);

                    //int start = Math.Max(rowStart, columnStart);
                    //int end = Math.Min(rowEnd, columnEnd);

                    //double correlationCoefficient;
                    //int intersectionLength = 0;

                    //if (start >= end)
                    //{
                    //    correlationCoefficient = 0.0;
                    //}
                    //else
                    //{
                    //    int length = end - start + 1;

                    //    double[] rowSubArray = new double[length];
                    //    double[] columnSubArray = new double[length];

                    //    Array.Copy(dataSet[row], start, rowSubArray, 0, length);
                    //    Array.Copy(dataSet[column], start, columnSubArray, 0, length);

                    //    correlationCoefficient = Correlation.Pearson(rowSubArray, columnSubArray);
                    //    intersectionLength = length;
                    //}

                    coefficientMatrix[row, column] = correlationCoefficient;
                    coefficientMatrix[column, row] = correlationCoefficient;

                    intersectionMatrix[row, column] = intersectionLength;
                    intersectionMatrix[column, row] = intersectionLength;
                }
            }

            // 3. clustering based on coefficient matrix
            // var clusters = Clustering(coefficientMatrix, threshold);

            // write to output file
            string intersectionFile = Path.Combine(
                Path.GetDirectoryName(outputFile),
                Path.GetFileNameWithoutExtension(outputFile) + "_intersection" + Path.GetExtension(outputFile));

            using (StreamWriter outputWriter = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                using (StreamWriter intersectionWriter = new StreamWriter(intersectionFile, false, Encoding.UTF8))
                {
                    string separator = ",";

                    // write header
                    string header = "N/A" + separator + string.Join(separator, codeAndNames);
                    outputWriter.WriteLine(header);
                    intersectionWriter.WriteLine(header);

                    // write metric
                    for (int i = 0; i < coefficientMatrix.RowCount; ++i)
                    {
                        string content = codeAndNames[i] + separator
                            + string.Join(
                                separator,
                                Enumerable
                                .Range(0, coefficientMatrix.ColumnCount)
                                .Select(j => coefficientMatrix[i, j].ToString()));

                        outputWriter.WriteLine(content);

                        string intersection = codeAndNames[i] + separator
                            + string.Join(
                                separator,
                                Enumerable
                                .Range(0, dataSetCount)
                                .Select(j => intersectionMatrix[i, j].ToString()));

                        intersectionWriter.WriteLine(intersection);
                    }
                }
            }
        }

        private static IEnumerable<int[]> Clustering(Matrix<double> coeffMatrix, double threshold)
        {
            return null;

            bool[] isClustered = new bool[coeffMatrix.RowCount];
            Array.Clear(isClustered, 0, isClustered.Length);

            do
            {
                int[] unclusteredRowIndices = Enumerable
                    .Range(0, isClustered.Count())
                    .Where(i => !isClustered[i])
                    .ToArray();

                if (unclusteredRowIndices.Length == 0)
                {
                    break;
                }

                // for any row that has no any coefficient (except with itself) great than threshold, 
                // it should be an individual cluster.
                foreach (var rowIndex in unclusteredRowIndices)
                {
                    //if ()
                }

            } while (true);

            while (isClustered.Count(b => !b) > 0)
            {
                for (int i = 0; i < isClustered.Length; ++i)
                {
                    if (!isClustered[i])
                    {

                    }
                }
            }
        }

        private static double[] NormalizeData(HistoryData data, DateTime[] allDates)
        {
            double[] values = new double[allDates.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                values[i] = 0.0;
            }

            int currentTimeIndex = 0;
            int currentDataIndex = 0;

            while (currentTimeIndex < allDates.Length && currentDataIndex < data.DataOrderedByTime.Length)
            {
                var bar = data.DataOrderedByTime[currentDataIndex];
                var currentTime = allDates[currentTimeIndex];

                if (bar.Time == currentTime)
                {
                    values[currentTimeIndex] = bar.ClosePrice;
                    currentTimeIndex++;
                    currentDataIndex++;
                }
                else if (bar.Time < currentTime)
                {
                    currentDataIndex++;
                }
                else // (bar.Time > currentTime)
                {
                    currentTimeIndex++;
                }
            }

            return values;
        }
    }
}
