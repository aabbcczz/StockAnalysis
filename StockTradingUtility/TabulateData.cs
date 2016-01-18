using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class TabulateData
    {
        private string[] _columns;
        private Dictionary<string, int> _columnNameToIndexMap = new Dictionary<string, int>();
        private List<string[]> _rows = new List<string[]>();

        public IEnumerable<string> Columns
        {
            get { return _columns; }
        }

        public int RowCount
        {
            get { return _rows.Count; }
        }

        public IEnumerable<string[]> Rows
        {
            get { return _rows; }
        }

        /// <summary>
        /// Get index for given column
        /// </summary>
        /// <param name="column">name of column</param>
        /// <returns>index of column (starts from 0) if column exists, otherwise -1</returns>
        public int GetColumnIndex(string column)
        {
            int index;
            if (_columnNameToIndexMap.TryGetValue(column, out index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

        public TabulateData(IEnumerable<string> columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException();
            }

            _columns = columns.ToArray();

            for (int i = 0; i < _columns.Length; ++i)
            {
                _columnNameToIndexMap[_columns[i]] = i;
            }
        }

        public TabulateData GetSubColumns(IEnumerable<string> columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException();
            }

            if (columns.Count() == 0)
            {
                // we can't return a sub tabulate result with no column.
                return null;
            }

            var columnIndices = columns.Select(c => GetColumnIndex(c));

            TabulateData subResult = new TabulateData(columns);
            foreach (var row in _rows)
            {
                var subRow = columnIndices.Select(i => i < 0 ? string.Empty : row[i]);
                
                subResult.AddRow(subRow);
            }

            return subResult;
        }

        public TabulateData GetSubColumns(IEnumerable<int> columnIndices)
        {
            if (columnIndices == null)
            {
                throw new ArgumentNullException();
            }

            if (columnIndices.Count() == 0)
            {
                // we can't return a sub tabulate result with no column.
                return null;
            }

            if (columnIndices.Any(i => i < 0 || i >= _columns.Length))
            {
                throw new ArgumentException("column index is out of range");
            }

            var columns = columnIndices.Select(i => _columns[i]);

            TabulateData subResult = new TabulateData(columns);
            foreach (var row in _rows)
            {
                var subRow = columnIndices.Select(i => row[i]);

                subResult.AddRow(subRow);
            }

            return subResult;
        }

        public TabulateData GetSubRows(IEnumerable<int> rowIndices)
        {
            if (rowIndices == null)
            {
                throw new ArgumentNullException();
            }

            var validRowIndices = rowIndices.Where(i => i >= 0 && i < RowCount);

            // even if there is no valid row index, we still need to return a sub tabulate result
            // with no row.
            TabulateData subResult = new TabulateData(_columns);
            foreach (var index in validRowIndices)
            {
                subResult.AddRow(_rows[index]);
            }

            return subResult;
        }

        public bool AddRow(IEnumerable<string> row)
        {
            if (row.Count() != _columns.Count())
            {
                return false;
            }

            _rows.Add(row.ToArray());

            return true;
        }

        public static bool TryParse(string s, out TabulateData result)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException();
            }

            // split rows firstly.
            string[] rows = s.Split('\n');

            // row[0] is the header
            string[] columns = rows[0].Split('\t');

            result = new TabulateData(columns);

            for (int i = 1; i < rows.Length; ++i)
            {
                string[] values = rows[i].Split('\t');

                if (!result.AddRow(values))
                {
                    return false;
                }
            }

            return true;
        }

        public static TabulateData Parse(string s)
        {
            TabulateData result;

            if (!TryParse(s, out result))
            {
                throw new InvalidOperationException("Failed to parse from string");
            }

            return result;
        }
    }
}
