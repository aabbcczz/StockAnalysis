using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Web;

using StockAnalysis.Share;

namespace ReportParser
{
    class EastMoneyPlainHtmlReportParser : IReportParser
    {
        private const string EffectiveRowStartPattern = "│";
        private const string RowCellSeparator = "│";
        private const string ReportHeaderStartPattern = "≈≈";
        private const string TableNameStartPattern = "◆";
        private const string TableFirstRowStartPattern = "┌─";
        private const string TableSectionSeparatorStartPattern = "├";
        private const string TableLastRowStartPattern = "└─";

        private TextWriter _errorWriter = null;
        private DataDictionary _dataDictionary = null;

        public EastMoneyPlainHtmlReportParser(DataDictionary dataDictionary, TextWriter errorWriter)
        {
            if (errorWriter == null)
            {
                throw new ArgumentNullException("errorWriter");
            }

            if (dataDictionary == null)
            {
                throw new ArgumentNullException("dataDictionary");
            }

            _errorWriter = errorWriter;
            _dataDictionary = dataDictionary;
        }

        public FinanceReport ParseReport(string code, string file)
        {
            List<string> lines = new List<string>();

            foreach (var line in Parse(file))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                lines.Add(line.Trim());
            }

            // recursive descending analysis
            return ParseReport(code, lines.ToArray());
        }

        private FinanceReport ParseReport(string code, string[] lines)
        {
            FinanceReport report = new FinanceReport();

            int lineIndex = 0;
            
            string companyName;
            if (!ParseHeader(code, lines, ref lineIndex, out companyName))
            {
                _errorWriter.WriteLine("Failed to parse report header");
                return null;
            }

            report.CompanyCode = code;
            report.CompanyName = companyName;

            while (lineIndex < lines.Length)
            {
                int currentLineIndex = lineIndex;

                FinanceReportTable table = ParseTable(lines, ref lineIndex);

                if (table != null && table.Name != "环比分析")
                {
                    report.AddTable(table);
                }
                else
                {
                    report.Annotations = string.Join(
                        Environment.NewLine, 
                        Enumerable
                        .Range(currentLineIndex, lines.Length - currentLineIndex)
                        .Select(i => lines[i]));

                    break;
                }
            }

            return report;
        }

        private bool Expect(string[] lines, ref int lineIndex, string expectedPattern, string nextExpectedPattern = null)
        {
            while (lineIndex < lines.Length)
            {
                string currentLine = lines[lineIndex];

                if (!string.IsNullOrWhiteSpace(currentLine))
                {
                    if (!string.IsNullOrEmpty(nextExpectedPattern))
                    {
                        // determine if current line match the next expected pattern before matching expected pattern.
                        if (currentLine.StartsWith(nextExpectedPattern))
                        {
                            _errorWriter.WriteLine("expected pattern [{0}] is not found", expectedPattern);
                            return false;
                        }
                    }

                    if (currentLine.StartsWith(expectedPattern))
                    {
                        return true;
                    }
                }

                lineIndex++;
            }

            return false;
        }

        private bool ParseHeader(string code, string[] lines, ref int lineIndex, out string companyName)
        {
            companyName = string.Empty;

            if (Expect(lines, ref lineIndex, ReportHeaderStartPattern, TableNameStartPattern))
            {
                string currentLine = lines[lineIndex];

                int posOfCode = currentLine.IndexOf(code);
                if (posOfCode >= 0)
                {
                    companyName = currentLine.Substring(0, posOfCode).Replace('≈', ' ').Trim();
                }
                else
                {
                    string[] fields = currentLine.Split(new char[] { '≈' }, StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length > 0)
                    {
                        companyName = fields[0];
                    }
                }

                lineIndex++;
                return true;
            }

            return false;
        }

        private bool ParseTableName(string[] lines, ref int lineIndex, out string tableName)
        {
            tableName = string.Empty;
            if (!Expect(lines, ref lineIndex, TableNameStartPattern, TableFirstRowStartPattern))
            {
                return false;
            }

            string currentLine = lines[lineIndex];
            lineIndex++;

            tableName = currentLine.Replace(TableNameStartPattern, string.Empty).Trim();

            int pos = tableName.IndexOf("（");
            if (pos >= 0)
            {
                tableName = tableName.Substring(0, pos);
            }

            return true;
        }

        private bool ParseTableDefinition(string[] lines, ref int lineIndex, out string rowDefinition, out string[] columnDefinitions)
        {
            rowDefinition = string.Empty;
            columnDefinitions = null;

            if (!Expect(lines, ref lineIndex, TableFirstRowStartPattern, EffectiveRowStartPattern))
            {
                return false;
            }

            lineIndex++;
            if (!Expect(lines, ref lineIndex, EffectiveRowStartPattern, TableSectionSeparatorStartPattern))
            {
                return false;
            }

            string currentLine = lines[lineIndex];
            lineIndex++;

            string[] fields = currentLine.Split(new char[] { '│' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 2)
            {
                return false;
            }

            rowDefinition = fields[0].Trim();
            columnDefinitions = fields.Skip(1).Select(x => x.Trim()).ToArray();

            return true;
        }

        private string[][] CleanUpCells(string[][] cells, int tableColumnCount)
        {
            if (cells.Length == 0)
            {
                return cells;
            }

            // get valid lines
            List<string[]> cleanedCells = new List<string[]>();

            int nextRow = 0;
            int currentRow = 0;

            do
            {
                currentRow = nextRow;
                nextRow = currentRow + 1;

                if (cells[currentRow].Length == 2
                    && !string.IsNullOrWhiteSpace(cells[currentRow][0])
                    && string.IsNullOrWhiteSpace(cells[currentRow][1]))
                {
                    //├───────────┼─────┴─────┴─────┴─────┤
                    //│*资本构成* │                       │
                    //├───────────┼─────┬─────┬─────┬─────┤

                    // skip this line.
                    continue;
                }

                if (cells[currentRow].Length != tableColumnCount + 1)
                {
                    // row with wrong column number, skip it.
                    continue;
                }

                if (string.IsNullOrWhiteSpace(cells[currentRow][0])
                    || Enumerable.Range(1, cells[currentRow].Length - 1).All(j => string.IsNullOrWhiteSpace(cells[currentRow][j])))
                {
                    // wrong line, skip it.
                    continue;
                }

                // scan for broken rows
                while (nextRow < cells.Length)
                {
                    // skip invalid lines
                    if (cells[nextRow].Length != cells[currentRow].Length)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(cells[nextRow][0])
                        || Enumerable.Range(1, cells[nextRow].Length - 1).All(column => string.IsNullOrWhiteSpace(cells[nextRow][column])))
                    {
                        //│审计意见              │        --│标准无保留│标准无保留│标准无保留│
                        //│                      │          │      意见│      意见│      意见│
                        //
                        // or
                        //
                        //│可供出售金融资│    44143.46│    35563.38│    38602.83│    40503.73│
                        //│产            │            │            │            │            │


                        // merge with current row;
                        for (int column = 0; column < cells[nextRow].Length; ++column)
                        {
                            cells[currentRow][column] += cells[nextRow][column];
                        }

                        nextRow++;
                        continue;
                    }

                    break;
                }

                cleanedCells.Add(cells[currentRow]);
            }
            while (nextRow < cells.Length);

            return cleanedCells.ToArray();
        }

        private FinanceReportTable ParseTable(string[] lines, ref int lineIndex)
        {
            const string UnknownTableName = "<unknown table name>";

            string tableName;

            // parse table name
            int startLineIndex = lineIndex;
            if (!ParseTableName(lines, ref lineIndex, out tableName))
            {
                _errorWriter.WriteLine("failed to find table name between line {0}~{1}", startLineIndex, lineIndex);
                tableName = UnknownTableName;
            }

            if (tableName != UnknownTableName)
            {
                // get normalized table name
                tableName = _dataDictionary.GetNormalizedTableName(tableName);
            }

            // parse table definition, include row definiton and column definition
            string rowDefinition;
            string[] columnDefinitions;

            startLineIndex = lineIndex;
            if (!ParseTableDefinition(lines, ref lineIndex, out rowDefinition, out columnDefinitions))
            {
                _errorWriter.WriteLine("failed to parse table definition from line {0}", startLineIndex);

                return null;
            }

            // get normalized column definitions
            columnDefinitions = columnDefinitions.Select(s => _dataDictionary.GetNormalizedColumnName(tableName, s)).ToArray();

            FinanceReportTable table = new FinanceReportTable(tableName, rowDefinition, columnDefinitions);

            // find out all possible rows
            startLineIndex = lineIndex;
            while (lineIndex < lines.Length)
            {
                string currentLine = lines[lineIndex];

                if (currentLine.StartsWith(TableLastRowStartPattern)) // end of table
                {
                    lineIndex++;
                    break;
                }

                if (currentLine.StartsWith(EffectiveRowStartPattern) || currentLine.StartsWith(TableSectionSeparatorStartPattern))
                {
                    lineIndex++;
                    continue;
                }

                // not a valid line in table
                break;
            }

            int endLineIndex = lineIndex;

            if (startLineIndex == endLineIndex)
            {
                // empty table
                return table;
            }

            // get all cells
            string[][] cells =
                Enumerable
                .Range(startLineIndex, endLineIndex - startLineIndex)
                .Select(i => lines[i])
                .SkipWhile(s => !s.StartsWith(EffectiveRowStartPattern))
                .Select(s => s.Split(new string[] { RowCellSeparator }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray())
                .ToArray();

            // clean up cells
            cells = CleanUpCells(cells, table.ColumnCount);

            FinanceReportColumnDefinition[] tableColumnDefinitions = table.ColumnDefinitions.ToArray();

            // create rows and adjust cell values according to unit.
            bool isHbfxTable = (table.Name == "环比分析");

            // last chance of getting table name according to row names.
            if (table.Name == UnknownTableName)
            {

                var tableNamesList = cells
                    .Select(rc => isHbfxTable ? table.RowDefinition + rc[0] : rc[0])
                    .Select(rowName => _dataDictionary.GetPossibleNormalizedTableNameByRowNameAlias(GetCleanedRowName(rowName)))
                    .ToList();

                IEnumerable<string> tableNames = new List<string>();

                if (tableNamesList.Count == 1)
                {
                    tableNames = tableNamesList[0];
                }
                else if (tableNamesList.Count > 1)
                {
                    tableNames = tableNamesList[0];

                    for (int i = 1; i < tableNamesList.Count; ++i)
                    {
                        tableNames = tableNames.Intersect(tableNamesList[i]);
                    }
                }

                if (tableNames.Count() == 0)
                {
                    _errorWriter.WriteLine("failed to guess table name from row names, no table contains all row names");

                    return null;
                }
                else if (tableNames.Count() > 1)
                {
                    _errorWriter.WriteLine("failed to guess table name from row names, more than one tables contain all row names");

                    return null;
                }

                // now we can set the table name to the unique possibility
                table.ResetTableName(tableNames.First());
                _errorWriter.WriteLine("find table name {0} from row names", table.Name);
            }

            foreach (var rowCells in cells)
            {
                string rowName = isHbfxTable ? table.RowDefinition + rowCells[0] : rowCells[0];

                // get normalized row name
                rowName = _dataDictionary.GetNormalizedRowName(table.Name, rowName);

                int rowIndex = table.AddRow(rowName);

                //if (rowCells[0] == "筹资活动产生的现金流出小计")
                //{
                //    Console.WriteLine("..........");
                //    Console.ReadKey();
                //}

                FinanceReportRow row = table[rowIndex];

                for (int i = 0; i < row.Length; ++i)
                {
                    row[i].Parse(rowCells[i + 1], tableColumnDefinitions[i].HasUnit ? tableColumnDefinitions[i].Unit : row.Unit);
                }
            }

            return table;
        }

        private string GetCleanedRowName(string rowName)
        {
            string cleanedName;
            decimal unit;

            FinanceReportHelper.ParseDefinitionAndUnit(rowName, 1.0M, out cleanedName, out unit);

            return cleanedName;
        }

        private IEnumerable<string> Parse(string file)
        {
            string content = null;
            using (StreamReader reader = new StreamReader(file))
            {
                content = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                yield break;
            }

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);

            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes(@"//div[@id=""maincontent""]");

            if (nodes == null || nodes.Count != 1)
            {
                Console.WriteLine("Can't find proper content to parse");
                yield break;
            }

            foreach (var node in nodes[0].ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    yield return HttpUtility.HtmlDecode(node.InnerText);
                }
            }
        }
    }
}
