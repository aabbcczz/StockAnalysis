using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using StockAnalysis.Share;

namespace ReportParser
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

                Run(options);
            }
        }

        static void Run(Options options)
        {
            options.Print(Console.Out);

            string inputFolder = Path.GetFullPath(options.InputFolder);

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("Input file folder {0} does not exist.", inputFolder);
                return;
            }

            DataDictionary dataDictionary = new DataDictionary();

            if (!string.IsNullOrEmpty(options.DataDictionaryFile))
            {
                dataDictionary.Load(options.DataDictionaryFile);
            }

            DataDictionary newDataDictionary = new DataDictionary(dataDictionary);

            IReportParser parser = ReportParserFactory.Create(options.FinanceReportFileType, dataDictionary, Console.Error);

            List<FinanceReport> reports = new List<FinanceReport>();

            foreach (var file in Directory.EnumerateFiles(inputFolder))
            {
                string code = Path.GetFileNameWithoutExtension(file);

                if (string.IsNullOrWhiteSpace(code))
                {
                    Console.WriteLine("The file name {0} is not expected", file);
                }

                FinanceReport report = parser.ParseReport(code, file);

                if (report == null)
                {
                    Console.WriteLine("Parse report file {0} failed.", file);
                }
                else
                {
                    Console.WriteLine("Parse report for {0}:{1} succeeded.", report.CompanyCode, report.CompanyName);

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
                HashSet<string> uniqueRowNames = new HashSet<string>();
                List<string> orderedRowNames = new List<string>();

                foreach (var table in tables)
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
                HashSet<string> uniqueColumnTexts = new HashSet<string>();
                List<string> orderedColumnTexts = new List<string>();
                foreach (var table in tables)
                {
                    foreach (var columnDefinition in table.ColumnDefinitions)
                    {
                        if (columnDefinition.Type == FinanceReportColumnDefinition.ColumnType.Text)
                        {
                            string columnText = columnDefinition.Text;

                            if (!uniqueColumnTexts.Contains(columnText))
                            {
                                uniqueColumnTexts.Add(columnText);
                                orderedColumnTexts.Add(columnText);
                            }
                        }
                    }
                }

                List<DateTime> orderedColumnDate = tables
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
            var uniqueTableNames = reports
                .SelectMany(r => r.Tables.Select(t => t.Name))
                .GroupBy(k => k)
                .Select(g => g.Key);

            if (uniqueTableNames.Count() == 0)
            {
                return;
            }

            foreach (var tableName in uniqueTableNames)
            {
                string outputFileName = Path.Combine(outputFolder, tableName + ".csv");

                using (StreamWriter writer = new StreamWriter(outputFileName, false, Encoding.UTF8))
                {
                    var tables = reports.SelectMany(r => r.Tables.Where(t => t.Name == tableName));
                    if (tables.Count() == 0)
                    {
                        continue;
                    }

                    List<string> outputColumnsForHeader = new List<string>();
                    outputColumnsForHeader.Add("CODE");
                    outputColumnsForHeader.Add("PeriodOrColumn");
                    outputColumnsForHeader.AddRange(tables.First().Rows.Select(r => r.Name));

                    // write header
                    writer.WriteLine(string.Join(",", outputColumnsForHeader));

                    foreach (var report in reports)
                    {
                        string code = GetNormalizedCode(report.CompanyCode);

                        foreach (var table in report.Tables.Where(t => t.Name == tableName))
                        {
                            string[] tableColumnNames = table.ColumnDefinitions
                                .Select(c => (c.Type == FinanceReportColumnDefinition.ColumnType.Date) ? c.Date.ToString("yyyy-MM-dd") : c.Text)
                                .ToArray();

                            for (int i = 0; i < tableColumnNames.Length; ++i)
                            {
                                if (table.Rows.All(r => r[i].Type == FinanceReportCell.CellType.NotApplicable))
                                {
                                    continue;
                                }

                                List<string> outputColumnsForRow = new List<string>();

                                outputColumnsForRow.Add(code);

                                outputColumnsForRow.Add(tableColumnNames[i]);

                                foreach (var row in table.Rows)
                                {
                                    string s = row[i].ToString();
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

        private static string GetNormalizedCode(string code)
        {
            StockName name = StockName.Parse(code);

            string prefix = string.Empty;
            if (name.Market == StockExchangeMarket.ShangHai)
            {
                prefix = "SH";
            }
            else if (name.Market == StockExchangeMarket.ShengZhen)
            {
                prefix = "SZ";
            }

            return prefix + name.Code;
        }

        private static void CreateRevenueTableForLast12Months(IEnumerable<FinanceReport> reports)
        {
            foreach (var report in reports)
            {
                // assume the tables in report has been expanded and merged.
                var tables = report.Tables.Where(t => t.Name == "利润表");
                if (tables.Count() == 0)
                {
                    continue;
                }

                if (tables.Count() > 1)
                {
                    throw new InvalidOperationException(
                        string.Format("there are more than one revenue table in the report for company {0}", report.CompanyCode));
                }

                var table = tables.First();

                FinanceReportColumnDefinition[] columns = table.ColumnDefinitions.ToArray();
                for (int i = 0; i < columns.Length; ++i)
                {
                    columns[i].Tag = i;
                }

                FinanceReportColumnDefinition[] dateColumns = columns
                    .Where(c => c.Type == FinanceReportColumnDefinition.ColumnType.Date)
                    .OrderByDescending(c => c.Date)
                    .ToArray();

                if (dateColumns.Length == 0)
                {
                    continue;
                }

                // find first non-empty column (it is important to avoid outliers)
                FinanceReportColumnDefinition firstActiveColumn = null;
                for (int i = 0; i < dateColumns.Length; ++i)
                {
                    if (!table.Rows.All(r => r[dateColumns[i].Tag].Type != FinanceReportCell.CellType.Decimal)
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

                FinanceReportColumnDefinition[] newColumns = new FinanceReportColumnDefinition[] { firstActiveColumn };

                FinanceReportTable newRevenueTable 
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
                        int rowIndex = newRevenueTable.AddRow(row.Name);
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
                    int firstColumnIndex = firstActiveColumn.Tag;

                    int secondColumnIndex = -1;
                    for (int i = 0; i < dateColumns.Length; ++i)
                    {
                        if (dateColumns[i].Date.Year == firstActiveColumn.Date.Year - 1
                            && dateColumns[i].Date.Month == 12)
                        {
                            secondColumnIndex = dateColumns[i].Tag;
                            break;
                        }
                    }

                    if (secondColumnIndex < 0)
                    {
                        // skip new table, just return;
                        return;
                    }

                    int thirdColumnIndex = -1;
                    for (int i = 0; i < dateColumns.Length; ++i)
                    {
                        if (dateColumns[i].Date.Year == firstActiveColumn.Date.Year - 1
                            && dateColumns[i].Date.Month == firstActiveColumn.Date.Month)
                        {
                            thirdColumnIndex = dateColumns[i].Tag;
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
                        decimal v1 = GetCellDecimalValue(row[firstColumnIndex]);
                        decimal v2 = GetCellDecimalValue(row[secondColumnIndex]);
                        decimal v3 = GetCellDecimalValue(row[thirdColumnIndex]);

                        int rowIndex = newRevenueTable.AddRow(row.Name);
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
