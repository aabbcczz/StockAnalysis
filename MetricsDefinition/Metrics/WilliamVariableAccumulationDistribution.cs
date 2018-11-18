namespace StockAnalysis.MetricsDefinition.Metrics
{
    using Common.Data;

    [Metric("WVAD")]
    public sealed class WilliamVariableAccumulationDistribution : SingleOutputBarInputSerialMetric
    {
        private readonly MovingSum _ms;

        public WilliamVariableAccumulationDistribution(int windowSize)
            : base(0)
        {
            _ms = new MovingSum(windowSize);
        }

        public override void Update(Bar bar)
        {
            var index = (bar.ClosePrice - bar.OpenPrice) * bar.Volume / (bar.HighestPrice - bar.LowestPrice);

            _ms.Update(index);

            SetValue(_ms.Value);
        }
    }
}
