using System;
using StockAnalysis.Common.Data;

namespace MetricsDefinition
{
    sealed class CallOperator : MetricBinaryOperator
    {
        private readonly StandaloneMetric _caller;
        private readonly MetricExpression _callee;

        public override double[] Values
        {
            get { return _caller.Values; }
        }

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

        public override void SingleOutputUpdate(double data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                _callee.SingleOutputUpdate(data);

                _caller.SingleOutputUpdate(_callee.Value);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }

        public override void MultipleOutputUpdate(double data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                _callee.SingleOutputUpdate(data);

                _caller.MultipleOutputUpdate(_callee.Value);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }

        public override void SingleOutputUpdate(Bar data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                _callee.SingleOutputUpdate(data);

                _caller.SingleOutputUpdate(_callee.Value);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }

        public override void MultipleOutputUpdate(Bar data)
        {
            if (_callee.FieldNames.Length <= 1)
            {
                _callee.SingleOutputUpdate(data);

                _caller.MultipleOutputUpdate(_callee.Value);
            }
            else
            {
                throw new InvalidOperationException(
                    "callee has multiple outputs, and caller can't handle it");
            }
        }
    }
}
