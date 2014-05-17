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
        
        public SelectionOperator(MetricExpression host, int fieldIndex)
            : base(host)
        {
            if (fieldIndex < 0)
            {
                throw new ArgumentException("fieldIndex must not be negative");
            }

            _fieldIndex = fieldIndex;
        }
    
        public override IEnumerable<double>[] Evaluate(IEnumerable<double>[] data)
        {
            IEnumerable<double>[] result = Operand.Evaluate(data);

            if (result.Length <= _fieldIndex)
            {
                throw new ArgumentException("result data has no enough fields");
            }

            return new IEnumerable<double>[1] { result[_fieldIndex] };
        }

        public override string[] GetFieldNames()
        {
            return new string[1] { Operand.GetFieldNames()[_fieldIndex] }; 
        }
    }
}
