namespace StockAnalysis.FinancialReportUtility
{
    using System;
    public sealed class FinancialReportCell
    {
        public enum CellType
        {
            Decimal,
            Text,
            NotApplicable
        }

        public CellType Type { get; private set; }

        private Object _value;

        public decimal DecimalValue 
        { 
            get 
            {
                return (decimal)_value; 
            }

            set
            {
                Type = CellType.Decimal;
                _value = value;
            }
        }

        public string StringValue
        {
            get
            {
                return (string)_value;
            }

            set
            {
                Type = CellType.Text;
                _value = value;
            }
        }

        public FinancialReportCell()
        {
            Type = CellType.NotApplicable;
        }

        public void Parse(string content, decimal unit)
        {
            decimal result;

            if (content.StartsWith("--"))
            {
                Type = CellType.NotApplicable;
                _value = null;
            }
            else if (decimal.TryParse(content, out result))
            {
                DecimalValue = result * unit;
            }
            else
            {
                StringValue = content;
            }
        }

        public void Copy(FinancialReportCell cell)
        {
            Type = cell.Type;
            _value = cell._value;
        }

        public void Merge(FinancialReportCell cell)
        {
            if (Type == CellType.NotApplicable)
            {
                Copy(cell);
            }
            else
            {
                if (cell.Type == CellType.NotApplicable)
                {
                    return;
                }
                Copy(cell);
            }
        }

        public override string ToString()
        {
            if (Type == CellType.NotApplicable)
            {
                return string.Empty;
            }
            return _value.ToString();
        }
    }
}
