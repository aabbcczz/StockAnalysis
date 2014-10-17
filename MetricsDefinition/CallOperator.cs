using System;
using StockAnalysis.Share;

namespace MetricsDefinition
{
    sealed class CallOperator : MetricBinaryOperator
    {
        private readonly StandaloneMetric _caller;
        private readonly MetricExpression _callee;

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
            if (_callee.FieldNames.Length <= 1)
            {
                double calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.SingleOutputUpdate(calleeResult);
            }

            throw new InvalidOperationException(
                "callee has multiple outputs, and caller can't handle it");
        }

        public override double[] MultipleOutputUpdate(double data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                double calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.MultipleOutputUpdate(calleeResult);
            }

            throw new InvalidOperationException(
                "callee has multiple outputs, and caller can't handle it");
        }

        public override double SingleOutputUpdate(Bar data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                double calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.SingleOutputUpdate(calleeResult);
            }

            throw new InvalidOperationException(
                "callee has multiple outputs, and caller can't handle it");
        }

        public override double[] MultipleOutputUpdate(Bar data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                double calleeResult = _callee.SingleOutputUpdate(data);

                return _caller.MultipleOutputUpdate(calleeResult);
            }

            throw new InvalidOperationException(
                "callee has multiple outputs, and caller can't handle it");
        }
    }
}
