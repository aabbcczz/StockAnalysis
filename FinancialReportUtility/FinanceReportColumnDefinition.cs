namespace StockAnalysis.FinancialReportUtility
{
    using System;
    public sealed class FinanceReportColumnDefinition
    {
        public enum ColumnType
        {
            Date,
            Text
        }

        public ColumnType Type { get; private set; }

        private object _columnDefinition;

        /// <summary>
        /// If the column has unit.
        /// </summary>
        public bool HasUnit { get; private set; }

        public int Tag { get; set; }

        private readonly decimal _unit;
        public decimal Unit 
        {
            get 
            { 
                if (!HasUnit)
                {
                    throw new NotSupportedException();
                }

                return _unit;
            }
        }

        public DateTime Date
        {
            get { return (DateTime)_columnDefinition; }
        }

        public string Text
        {
            get { return (string)_columnDefinition; }
        }

        public FinanceReportColumnDefinition(string columnDefinition)
        {
            if (columnDefinition == null)
            {
                throw new ArgumentNullException("columnDefinition");
            }

            DateTime date;

            if (DateTime.TryParse(columnDefinition, out date))
            {
                Type = ColumnType.Date;
                _columnDefinition = date;
                HasUnit = false;
            }
            else
            {
                Type = ColumnType.Text;

                string cleanedColumnDefinition;
                HasUnit = FinanceReportHelper.ParseDefinitionAndUnit(columnDefinition, 1.0M, out cleanedColumnDefinition, out _unit);

                _columnDefinition = cleanedColumnDefinition;
            }
        }

        public FinanceReportColumnDefinition(DateTime date)
        {
            Type = ColumnType.Date;
            _columnDefinition = date;
            HasUnit = false;
        }

        public void Normalize(DataDictionary dictionary, string tableName)
        {
            if (Type == ColumnType.Text && !string.IsNullOrEmpty(Text))
            {
                _columnDefinition = dictionary.GetNormalizedColumnName(tableName, Text);
            }
        }
    }
}
