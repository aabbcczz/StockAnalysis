namespace StockAnalysis.FinancialReportUtility
{
    using System;
    using System.Collections.Generic;

    public sealed class FinanceReportRow
    {
        private readonly FinanceReportCell[] _cells;

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

        public FinanceReportRow(string name, ICollection<FinanceReportColumnDefinition> columnDefinitions, decimal defaultUnit)
        {
            if (columnDefinitions == null || columnDefinitions.Count == 0)
            {
                throw new ArgumentNullException("columnDefinitions");
            }

            _cells = new FinanceReportCell[columnDefinitions.Count];
            for (var i = 0; i < _cells.Length; ++i)
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
