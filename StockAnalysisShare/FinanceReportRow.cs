using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class FinanceReportRow
    {
        private readonly FinanceReportColumnDefinition[] _columnDefinitions;

        private FinanceReportCell[] _cells;

        public FinanceReportCell this[int index]
        {
            get
            {
                if (index < 0 || index >= _cells.Length)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _cells[index];
            }
        }

        public int Length { get { return _cells.Length; } }

        public string Name { get; private set; }

        public decimal Unit { get; private set; }

        public FinanceReportRow(string name, FinanceReportColumnDefinition[] columnDefinitions, decimal defaultUnit)
        {
            if (columnDefinitions == null || columnDefinitions.Length == 0)
            {
                throw new ArgumentNullException("columnDefinitions");
            }

            _columnDefinitions = columnDefinitions;

            _cells = new FinanceReportCell[_columnDefinitions.Length];
            for (int i = 0; i < _cells.Length; ++i)
            {
                _cells[i] = new FinanceReportCell();
            }

            string cleanedName;
            decimal unit;

            FinanceReportHelper.ParseDefinitionAndUnit(name, defaultUnit, out cleanedName, out unit);

            Unit = unit;
            Name = cleanedName;
        }

        public void Normalize(DataDictionary dataDictionary, string tableName)
        {
            Name = dataDictionary.GetNormalizedRowName(tableName, Name);
        }
    }
}
