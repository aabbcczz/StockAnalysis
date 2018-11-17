namespace StockAnalysis.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    public sealed class CsvTable
    {
        private readonly string[] _header;
        private readonly List<string[]> _rows = new List<string[]>();
        public string[] Header
        {
            get { return _header; }
        }

        public string[] this[int rowIndex]
        {
            get { return _rows[rowIndex]; }
        }

        public IEnumerable<string[]> Rows
        {
            get { return _rows; }
        }

        public int RowCount { get { return _rows.Count; } }

        public int ColumnCount { get { return _header.Length; }}

        public CsvTable(string[] header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            _header = header;
        }

        public int AddRow(string[] row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            if (row.Length != ColumnCount)
            {
                throw new ArgumentException("the length of row does not match the header");
            }

            _rows.Add(row);

            return _rows.Count - 1;
        }

        public static CsvTable Load(string file, Encoding encoding, string separator, StringSplitOptions options = StringSplitOptions.None, int skipLineCount = 0, bool skipInvalidLine = true)
        {
            using (var reader = new StreamReader(file, encoding))
            {
                // skip first several lines
                for (var i = 0; i < skipLineCount; ++i)
                {
                    if (reader.ReadLine() == null)
                    {
                        return null;
                    }
                }

                // read header
                var headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    return null;
                }

                var splitter = new[] { separator };

                var header = headerLine.Split(splitter, options).Select(s => s.Trim()).ToArray();

                var csv = new CsvTable(header);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    var row = line.Split(splitter, options);

                    if (row.Length != header.Length)
                    {
                        if (!skipInvalidLine)
                        {
                            throw new InvalidDataException(string.Format("Invalid line: [{0}]", line));
                        }
                    }
                    else
                    {
                        csv.AddRow(row);
                    }
                }

                return csv;
            }
        }

        public static void Save(CsvTable csv, string file, Encoding encoding, string separator)
        {
            if (csv == null)
            {
                throw new ArgumentNullException("csv");
            }

            using (var writer = new StreamWriter(file, false, encoding))
            {
                writer.WriteLine(string.Join(separator, csv.Header));

                for (var i = 0; i < csv.RowCount; ++i)
                {
                    writer.WriteLine(string.Join(separator, csv[i]));
                }
            }
        }
    }
}
