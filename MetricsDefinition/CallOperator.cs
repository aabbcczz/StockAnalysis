using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    sealed class CallOperator : MetricBinaryOperator
    {
        private StandaloneMetric _caller;
        private MetricExpression _callee;

        public CallOperator(StandaloneMetric caller, MetricExpression callee)
            : base(caller, callee)
        {
            if (caller == null || callee == null)
            {
                throw new ArgumentNullException();
            }

            _caller = caller;
            _callee = callee;
        }

        public override double[][] Evaluate(double[][] data)
        {
            return _caller.Evaluate(_callee.Evaluate(data));
        }

        public override string[] GetFieldNames()
        {
            return _caller.GetFieldNames();
        }
    }
}
