using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalysis.Share
{
    public sealed class FinanceReportTable
    {
        private readonly FinanceReportColumnDefinition[] _columnDefinitions;

        private List<FinanceReportRow> _rows = new List<FinanceReportRow>();

        public int RowCount { get { return _rows.Count; } }

        public IEnumerable<FinanceReportRow> Rows { get { return _rows; } }

        public int ColumnCount { get { return _columnDefinitions.Length; } }

        public IEnumerable<FinanceReportColumnDefinition> ColumnDefinitions
        {
            get { return _columnDefinitions; }
        }

        public string Name { get; private set; }

        /// <summary>
        /// the top left of table which defines the meaning/cateogy of rows' name. unit, such as 万元, has been removed.
        /// </summary>
        public string RowDefinition { get; private set; }

        public FinanceReportRow this[int index]
        {
            get
            {
                if (index < 0 || index >= RowCount)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _rows[index];
            }
        }

        public decimal Unit { get; private set; }

        public FinanceReportTable(string name, string rowDefinition, decimal unit, FinanceReportColumnDefinition[] columnDefinitions)
        {
            if (columnDefinitions == null || columnDefinitions.Length == 0)
            {
                throw new ArgumentNullException("columnDefinitions");
            }

            Name = name;
            RowDefinition = rowDefinition;
            Unit = unit;

            _columnDefinitions = columnDefinitions;
        }

        public FinanceReportTable(string name, string rowDefinition, string[] columnDefinitions)
        {
            if (columnDefinitions == null || columnDefinitions.Length == 0)
            {
                throw new ArgumentNullException("columnDefinitions");
            }

            _columnDefinitions = new FinanceReportColumnDefinition[columnDefinitions.Length];
            for (var i = 0; i < _columnDefinitions.Length; ++i)
            {
                _columnDefinitions[i] = new FinanceReportColumnDefinition(columnDefinitions[i]);
            }

            Name = name;

            string cleanedRowDefinition;
            decimal unit;

            FinanceReportHelper.ParseDefinitionAndUnit(rowDefinition, 1.0M, out cleanedRowDefinition, out unit);

            Unit = unit;
            RowDefinition = cleanedRowDefinition;
        }

        public void ResetTableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
        }

        public int AddRow(string name)
        {
            var row = new FinanceReportRow(name, _columnDefinitions, Unit);
            _rows.Add(row);

            return _rows.Count - 1;
        }

        public void Normalize(DataDictionary dictionary)
        {
            Name = dictionary.GetNormalizedTableName(Name);

            foreach (var columnDefinition in _columnDefinitions)
            {
                columnDefinition.Normalize(dictionary, Name);
            }

            foreach (var row in _rows)
            {
                row.Normalize(dictionary, Name);
            }
        }

        public FinanceReportTable Expand(IList<string> orderedRowNames, IList<string> orderedColumnText, IList<DateTime> orderedColumnDate)
        {
            var columnDefinitions = new FinanceReportColumnDefinition[orderedColumnDate.Count + orderedColumnText.Count];

            for (var i = 0; i < orderedColumnDate.Count; ++i)
            {
                columnDefinitions[i] = new FinanceReportColumnDefinition(orderedColumnDate[i]);
            }

            for (var i = 0; i < orderedColumnText.Count; ++i)
            {
                columnDefinitions[i + orderedColumnDate.Count] = new FinanceReportColumnDefinition(orderedColumnText[i]);
            }

            var table = new FinanceReportTable(Name, RowDefinition, Unit, columnDefinitions);

            table._rows = new List<FinanceReportRow>();

            for (var i = 0; i < orderedRowNames.Count; ++i)
            {
                table._rows.Add(new FinanceReportRow(orderedRowNames[i], table._columnDefinitions, table.Unit));
            }

            // build old row index to new row index map and old column index to new column index
            var rowMap = new int[_rows.Count];
            for (var i = 0; i < _rows.Count; ++i)
            {
                rowMap[i] = orderedRowNames.IndexOf(_rows[i].Name);
            }

            var columnMap = new int[_columnDefinitions.Length];
            for (var i = 0; i < _columnDefinitions.Length; ++i)
            {
                if (_columnDefinitions[i].Type == FinanceReportColumnDefinition.ColumnType.Date)
                {
                    columnMap[i] = orderedColumnDate.IndexOf(_columnDefinitions[i].Date);
                }
                else
                {
                    columnMap[i] = orderedColumnText.IndexOf(_columnDefinitions[i].Text);
                    if (columnMap[i] >= 0)
                    {
                        columnMap[i] += orderedColumnDate.Count;
                    }
                }
            }

            // copy data from old table to new table
            for (var i = 0; i < _rows.Count; ++i)
            {
                for (var j = 0; j < _columnDefinitions.Length; ++j)
                {
                    table._rows[rowMap[i]][columnMap[j]].Copy(_rows[i][j]);
                }
            }

            return table;
        }

        public void Merge(FinanceReportTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            if (!table._rows.Select(r => r.Name).SequenceEqual(_rows.Select(r => r.Name)))
            {
                throw new InvalidOperationException("table's rows are inconsistent");
            }

            if (!table._columnDefinitions.SequenceEqual(_columnDefinitions, new ColumnDefinitionEqualityComparer()))
            {
                throw new InvalidOperationException("table's columns are inconsistent");
            }

            for (var i = 0; i < _rows.Count; ++i)
            {
                for (var j = 0; j < _columnDefinitions.Length; ++j)
                {
                    _rows[i][j].Merge(table._rows[i][j]);
                }
            }
        }

        private sealed class ColumnDefinitionEqualityComparer : EqualityComparer<FinanceReportColumnDefinition>
        {
            public override bool Equals(FinanceReportColumnDefinition x, FinanceReportColumnDefinition y)
            {
                if (x.Type == y.Type)
                {
                    if (x.Type == FinanceReportColumnDefinition.ColumnType.Text)
                    {
                        return x.Text == y.Text;
                    }
                    return x.Date == y.Date;
                }

                return false;
            }

            public override int GetHashCode(FinanceReportColumnDefinition obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}
