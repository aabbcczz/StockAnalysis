using System;
using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition
{
    sealed class SelectionOperator : MetricUnaryOperator
    {
        private readonly int _fieldIndex;
        private readonly string[] _fieldNames;
        private double[] _values;

        public override string[] FieldNames
        {
            get { return _fieldNames; }
        }

        public override double[] Values
        {
            get { return _values; }
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

            _values = new double[1];
        }

        public override void SingleOutputUpdate(double data)
        {
            if (Operand.FieldNames.Length > 1)
            {
                Operand.MultipleOutputUpdate(data);
                _values[0] = Operand.Values[_fieldIndex];
            }
            else
            {
                Operand.SingleOutputUpdate(data);
                _values[0] = Operand.Value;
            }
        }

        public override void MultipleOutputUpdate(double data)
        {
            Operand.MultipleOutputUpdate(data);
            _values[0] = Operand.Values[_fieldIndex];
        }

        public override void SingleOutputUpdate(Bar data)
        {
            if (Operand.FieldNames.Length > 1)
            {
                Operand.MultipleOutputUpdate(data);
                _values[0] = Operand.Values[_fieldIndex];
            }
            else
            {
                Operand.SingleOutputUpdate(data);
                _values[0] = Operand.Value;
            }
        }

        public override void MultipleOutputUpdate(Bar data)
        {
            Operand.MultipleOutputUpdate(data);
            _values[0] = Operand.Values[_fieldIndex];
        }
    }
}
