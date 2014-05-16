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
    
        public override IEnumerable<double> Evaluate(IEnumerable<StockAnalysis.Share.StockTransactionSummary> data)
        {
 	        throw new NotImplementedException();
        }

        public override IEnumerable<double> Evaluate(IEnumerable<double> data)
        {
 	        throw new NotImplementedException();
        }
    }
}
