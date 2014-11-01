using System;
using StockAnalysis.Share;

namespace MetricsDefinition
{
    sealed class SelectionOperator : MetricUnaryOperator
    {
        private readonly int _fieldIndex;
        private readonly string[] _fieldNames;

        public override string[] FieldNames
        {
            get { return _fieldNames; }
        }

        public SelectionOperator(MetricExpression host, int fieldIndex)
            : base(host)
        {
            if (fieldIndex < 0)
            {
                throw new ArgumentException("fieldIndex must not be negative");
            }

            if (fieldIndex >= host.FieldNames.Length)
            {
                throw new ArgumentException("fieldIndex is greater than host's total fields");
            }

            _fieldIndex = fieldIndex;

            _fieldNames = new[] { host.FieldNames[fieldIndex] };
        }

        public override double SingleOutputUpdate(double data)
        {
            if (Operand.FieldNames.Length > 1)
            {
                return Operand.MultipleOutputUpdate(data)[_fieldIndex];
            }
            else
            {
                return Operand.SingleOutputUpdate(data);
            }
        }

        public override double[] MultipleOutputUpdate(double data)
        {
            return new[] { Operand.MultipleOutputUpdate(data)[_fieldIndex] };
        }

        public override double SingleOutputUpdate(Bar data)
        {
            if (Operand.FieldNames.Length > 1)
            {
                return Operand.MultipleOutputUpdate(data)[_fieldIndex];
            }
            else
            {
                return Operand.SingleOutputUpdate(data);
            }
        }

        public override double[] MultipleOutputUpdate(Bar data)
        {
            return new[] { Operand.MultipleOutputUpdate(data)[_fieldIndex] };
        }
    }
}
