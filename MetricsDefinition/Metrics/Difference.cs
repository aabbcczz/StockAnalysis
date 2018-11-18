namespace StockAnalysis.MetricsDefinition.Metrics
{
    using Common.Data;

    [Metric("DIFF", "DIFF,PERCENT")]
    public sealed class Difference : MultipleOutputBarInputSerialMetric
    {
        private readonly MetricExpression _expression1;
        private readonly MetricExpression _expression2;

        public Difference(string metric1, string metric2)
            : base(0)
        {
            _expression1 = MetricEvaluationContext.ParseExpression(metric1);
            _expression2 = MetricEvaluationContext.ParseExpression(metric2);

            Values = new double[2];
        }

        public override void Update(Bar bar)
        {
            _expression1.MultipleOutputUpdate(bar);
            _expression2.MultipleOutputUpdate(bar);

            var v1 = _expression1.Value;
            var v2 = _expression2.Value;

            SetValue(v1 - v2, v1 / v2);
        }
    }
}
