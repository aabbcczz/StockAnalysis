using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CommandLine;

namespace ReportParser
{
    using StockAnalysis.Common.SymbolName;
    using StockAnalysis.Common.Exchange;
    using StockAnalysis.FinancialReportUtility;

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
        }

        static void Run(Options options)
        {
            options.Print(Console.Out);

            var inputFolder = Path.GetFullPath(options.InputFolder);

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("Input file folder {0} does not exist.", inputFolder);
                return;
            }

            var dataDictionary = new DataDictionary();

            if (!string.IsNullOrEmpty(options.DataDictionaryFile))
            {
                dataDictionary.Load(options.DataDictionaryFile);
            }

            var newDataDictionary = new DataDictionary(dataDictionary);

            var parser = ReportParserFactory.Create(options.FinanceReportFileType, dataDictionary, Console.Error);

            var reports = new List<FinanceReport>();

            foreach (var file in Directory.EnumerateFiles(inputFolder))
            {
                var symbol = Path.GetFileNameWithoutExtension(file);

                if (string.IsNullOrWhiteSpace(symbol))
                {
                    Console.WriteLine("The file name {0} is not expected", file);
                }

                var report = parser.ParseReport(symbol, file);

                if (report == null)
                {
                    Console.WriteLine("Parse report file {0} failed.", file);
                }
                else
                {
                    Console.WriteLine("Parse report for {0}:{1} succeeded.", report.CompanySymbol, report.CompanyName);

                    AddReportMetadataToDataDictionary(report, newDataDictionary);

                    reports.Add(report);
                }
            }

            // normalize each report according to data dictionary
            foreach (var report in reports)
            {
                report.Normalize(newDataDictionary);
            }

            // expand and merge tables in reports
            ExpandAndMergeTables(reports);

            // create revenue table for last 12 months
            CreateRevenueTableForLast12Months(reports);

            // output reports
            OutputReports(reports, options.OutputFolder);

            // save new data dictionary if necessary.
            if (!string.IsNullOrEmpty(options.GenerateDataDictionaryFile))
            {
                newDataDictionary.Save(options.GenerateDataDictionaryFile);
            }

            Console.WriteLine("Done.");
        }

        private static void AddReportMetadataToDataDictionary(FinanceReport report, DataDictionary dataDictionary)
        {
            foreach (var table in report.Tables)
            {
                dataDictionary.AddTableName(table.Name);

                foreach (var column in table.ColumnDefinitions)
                {
                    if (column.Type == FinanceReportColumnDefinition.ColumnType.Text && !string.IsNullOrWhiteSpace(column.Text))
                    {
                        dataDictionary.AddColumnName(table.Name, column.Text);
                    }
                }

                foreach (var row in table.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row.Name))
                    {
                        dataDictionary.AddRowName(table.Name, row.Name);
                    }
                }
            }
        }

        private static void ExpandAndMergeTables(IList<FinanceReport> reports)
        {
            // get all possible table names
            var uniqueTableNames = reports
                .SelectMany(r => r.Tables.Select(t => t.Name))
                .GroupBy(k => k)
                .Select(g => g.Key);

            // for each kind of table, get all possible row names and column definitions, and then expand and merge the tables in each report.
            foreach (var tableName in uniqueTableNames)
            {
                var tables = reports.SelectMany(r => r.Tables.Where(t => t.Name == tableName));

                // get all possible row names
                var uniqueRowNames = new HashSet<string>();
                var orderedRowNames = new List<string>();

                var financeReportTables = tables as FinanceReportTable[] ?? tables.ToArray();
                foreach (var table in financeReportTables)
                {
                    foreach (var rowName in table.Rows.Select(r => r.Name))
                    {
                        if (!uniqueRowNames.Contains(rowName))
                        {
                            uniqueRowNames.Add(rowName);
                            orderedRowNames.Add(rowName);
                        }
                    }
                }

                // get all possible column definitions
                var uniqueColumnTexts = new HashSet<string>();
                var orderedColumnTexts = new List<string>();
                foreach (var table in financeReportTables)
                {
                    foreach (var columnDefinition in table.ColumnDefinitions)
                    {
                        if (columnDefinition.Type == FinanceReportColumnDefinition.ColumnType.Text)
                        {
                            var columnText = columnDefinition.Text;

                            if (!uniqueColumnTexts.Contains(columnText))
                            {
                                uniqueColumnTexts.Add(columnText);
                                orderedColumnTexts.Add(columnText);
                            }
                        }
                    }
                }

                var orderedColumnDate = financeReportTables
                    .SelectMany(t => t.ColumnDefinitions)
                    .Where(c => c.Type == FinanceReportColumnDefinition.ColumnType.Date)
                    .Select(c => c.Date)
                    .GroupBy(dt => dt)
                    .Select(g => g.Key)
                    .OrderByDescending(dt => dt)
                    .ToList();

                foreach (var report in reports)
                {
                    report.ExpandAndMerge(tableName, orderedRowNames, orderedColumnTexts, orderedColumnDate);
                }
            }
        }

        private static void OutputReports(IEnumerable<FinanceReport> reports, string outputFolder)
        {
            // all reports has been expanded and merged.
            // get all possible table names
            var financeReports = reports as FinanceReport[] ?? reports.ToArray();
            var uniqueTableNames = financeReports
                .SelectMany(r => r.Tables.Select(t => t.Name))
                .GroupBy(k => k)
                .Select(g => g.Key);

            var tableNames = uniqueTableNames as string[] ?? uniqueTableNames.ToArray();
            if (!tableNames.Any())
            {
                return;
            }

            foreach (var tableName in tableNames)
            {
                var outputFileName = Path.Combine(outputFolder, tableName + ".csv");

                using (var writer = new StreamWriter(outputFileName, false, Encoding.UTF8))
                {
                    var tables = financeReports.SelectMany(r => r.Tables.Where(t => t.Name == tableName));
                    var financeReportTables = tables as FinanceReportTable[] ?? tables.ToArray();
                    if (!financeReportTables.Any())
                    {
                        continue;
                    }

                    var outputColumnsForHeader = new List<string> {"SYMBOL", "PeriodOrColumn"};
                    outputColumnsForHeader.AddRange(financeReportTables.First().Rows.Select(r => r.Name));

                    // write header
                    writer.WriteLine(string.Join(",", outputColumnsForHeader));

                    foreach (var report in financeReports)
                    {
                        var normalizedSymbol = GetNormalizedSymbol(report.CompanySymbol);

                        string name = tableName;
                        foreach (var table in report.Tables.Where(t => t.Name == name))
                        {
                            var tableColumnNames = table.ColumnDefinitions
                                .Select(c => (c.Type == FinanceReportColumnDefinition.ColumnType.Date) ? c.Date.ToString("yyyy-MM-dd") : c.Text)
                                .ToArray();

                            for (var i = 0; i < tableColumnNames.Length; ++i)
                            {
                                int i1 = i;
                                if (table.Rows.All(r => r[i1].Type == FinanceReportCell.CellType.NotApplicable))
                                {
                                    continue;
                                }

                                var outputColumnsForRow = new List<string> {normalizedSymbol, tableColumnNames[i]};

                                foreach (var row in table.Rows)
                                {
                                    var s = row[i].ToString();
                                    if (string.IsNullOrEmpty(s))
                                    {
                                        s = "0.0";
                                    }

                                    outputColumnsForRow.Add(s);
                                }

                                writer.WriteLine(string.Join(",", outputColumnsForRow));
                            }
                        }
                    }
                }
            }
        }

        private static string GetNormalizedSymbol(string symbol)
        {
            var name = StockName.Parse(symbol);

            return ExchangeFactory.GetExchangeById(name.Symbol.ExchangeId).CapitalizedSymbolPrefix + symbol;
        }

        private static void CreateRevenueTableForLast12Months(IEnumerable<FinanceReport> reports)
        {
            foreach (var report in reports)
            {
                // assume the tables in report has been expanded and merged.
                var tables = report.Tables.Where(t => t.Name == "利润表");
                var financeReportTables = tables as FinanceReportTable[] ?? tables.ToArray();
                if (!financeReportTables.Any())
                {
                    continue;
                }

                if (financeReportTables.Count() > 1)
                {
                    throw new InvalidOperationException(
                        string.Format("there are more than one revenue table in the report for company {0}", report.CompanySymbol));
                }

                var table = financeReportTables.First();

                var columns = table.ColumnDefinitions.ToArray();
                for (var i = 0; i < columns.Length; ++i)
                {
                    columns[i].Tag = i;
                }

                var dateColumns = columns
                    .Where(c => c.Type == FinanceReportColumnDefinition.ColumnType.Date)
                    .OrderByDescending(c => c.Date)
                    .ToArray();

                if (dateColumns.Length == 0)
                {
                    continue;
                }

                // find first non-empty column (it is important to avoid outliers)
                FinanceReportColumnDefinition firstActiveColumn = null;
                for (var i = 0; i < dateColumns.Length; ++i)
                {
                    int i1 = i;
                    if (table.Rows.Any(r => r[dateColumns[i1].Tag].Type == FinanceReportCell.CellType.Decimal)
                        && dateColumns[i].Date < DateTime.Now)
                    {
                        firstActiveColumn = dateColumns[i];
                        break;
                    }
                }

                if (firstActiveColumn == null)
                {
                    return;
                }

                var newColumns = new[] { firstActiveColumn };

                var newRevenueTable 
                    = new FinanceReportTable(
                        "跨年度利润表", 
                        table.RowDefinition, 
                        table.Unit, 
                        newColumns);

                if (firstActiveColumn.Date.Month == 12) // 年报
                {
                    // just copy rows
                    foreach (var row in table.Rows)
                    {
                        var rowIndex = newRevenueTable.AddRow(row.Name);
                        newRevenueTable[rowIndex][0].Copy(row[firstActiveColumn.Tag]);
                    }

                    report.AddTable(newRevenueTable);
                }
                else 
                {
                    // not yearly report, it could be seasonal report or half year report
                    // so we need to get last 12 month revenue data by 3 data:
                    // Last 12 month data = latest data + lastest annual report data - last year corresponding month data.
                    // for example:
                    //    data(2012/9~2013/9) = data(2013/9)+data(2012/12)-data(2012/9)
                    var firstColumnIndex = firstActiveColumn.Tag;

                    var secondColumnIndex = -1;
                    foreach (FinanceReportColumnDefinition t in dateColumns)
                    {
                        if (t.Date.Year == firstActiveColumn.Date.Year - 1
                            && t.Date.Month == 12)
                        {
                            secondColumnIndex = t.Tag;
                            break;
                        }
                    }

                    if (secondColumnIndex < 0)
                    {
                        // skip new table, just return;
                        return;
                    }

                    var thirdColumnIndex = -1;
                    foreach (FinanceReportColumnDefinition t in dateColumns)
                    {
                        if (t.Date.Year == firstActiveColumn.Date.Year - 1
                            && t.Date.Month == firstActiveColumn.Date.Month)
                        {
                            thirdColumnIndex = t.Tag;
                            break;
                        }
                    }

                    if (thirdColumnIndex < 0)
                    {
                        // skip new table, just return;
                        return;
                    }

                    foreach (var row in table.Rows)
                    {
                        var v1 = GetCellDecimalValue(row[firstColumnIndex]);
                        var v2 = GetCellDecimalValue(row[secondColumnIndex]);
                        var v3 = GetCellDecimalValue(row[thirdColumnIndex]);

                        var rowIndex = newRevenueTable.AddRow(row.Name);
                        newRevenueTable[rowIndex][0].DecimalValue = v1 + v2 - v3;
                    }

                    report.AddTable(newRevenueTable);

                }
            }
        }

        private static decimal GetCellDecimalValue(FinanceReportCell cell)
        {
            return cell.Type == FinanceReportCell.CellType.Decimal ? cell.DecimalValue : 0.0M;
        }
    }
}
