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

        public override string[] FieldNames
        {
            get { return _caller.FieldNames; }
        }

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

        public override double SingleOutputUpdate(double data)
        {
            double calleeResult;

            if (_callee.FieldNames.Length <= 1)
            {
                calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.SingleOutputUpdate(calleeResult);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }

        public override double[] MultipleOutputUpdate(double data)
        {
            double calleeResult;

            if (_callee.FieldNames.Length <= 1)
            {
                calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.MultipleOutputUpdate(calleeResult);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }

        public override double SingleOutputUpdate(StockAnalysis.Share.Bar data)
        {
            double calleeResult;

            if (_callee.FieldNames.Length <= 1)
            {
                calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.SingleOutputUpdate(calleeResult);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }

        public override double[] MultipleOutputUpdate(StockAnalysis.Share.Bar data)
        {
            double calleeResult;

            if (_callee.FieldNames.Length <= 1)
            {
                calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.MultipleOutputUpdate(calleeResult);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }
    }
}
