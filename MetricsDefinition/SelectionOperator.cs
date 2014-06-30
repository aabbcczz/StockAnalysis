using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    sealed class SelectionOperator : MetricUnaryOperator
    {
        private int _fieldIndex;
        private string[] _fieldNames;

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

            _fieldNames = new string[1] { host.FieldNames[fieldIndex] };
        }

        public override double SingleOutputUpdate(double data)
        {
            return Operand.SingleOutputUpdate(data);
        }

        public override double[] MultipleOutputUpdate(double data)
        {
            return new double[1] { Operand.MultipleOutputUpdate(data)[_fieldIndex] };
        }

        public override double SingleOutputUpdate(StockAnalysis.Share.Bar data)
        {
            return Operand.SingleOutputUpdate(data);
        }

        public override double[] MultipleOutputUpdate(StockAnalysis.Share.Bar data)
        {
            return new double[1] { Operand.MultipleOutputUpdate(data)[_fieldIndex] };
        }
    }
}
