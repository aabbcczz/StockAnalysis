using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StockAnalysis.Share
{
    public sealed class Csv
    {
        private string[] _header;
        private List<string[]> _rows = new List<string[]>();
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

        public Csv(string[] header)
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

        public static Csv Load(string file, Encoding encoding, string separator, StringSplitOptions options = StringSplitOptions.None, int skipLineCount = 0, bool skipInvalidLine = true)
        {
            using (StreamReader reader = new StreamReader(file, encoding))
            {
                // skip first several lines
                for (int i = 0; i < skipLineCount; ++i)
                {
                    if (reader.ReadLine() == null)
                    {
                        return null;
                    }
                }

                // read header
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    return null;
                }

                string[] splitter = new string[] { separator };

                string[] header = headerLine.Split(splitter, options).Select(s => s.Trim()).ToArray();

                Csv csv = new Csv(header);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    string[] row = line.Split(splitter, options);

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

        public static void Save(Csv csv, string file, Encoding encoding, string separator)
        {
            if (csv == null)
            {
                throw new ArgumentNullException("csv");
            }

            using (StreamWriter writer = new StreamWriter(file, false, encoding))
            {
                writer.WriteLine(string.Join(separator, csv.Header));

                for (int i = 0; i < csv.RowCount; ++i)
                {
                    writer.WriteLine(string.Join(separator, csv[i]));
                }
            }
        }
    }
}
