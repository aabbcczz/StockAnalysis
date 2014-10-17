using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("WVAD")]
    public sealed class WilliamVariableAccumulationDistribution : SingleOutputBarInputSerialMetric
    {
        private readonly MovingSum _ms;

        public WilliamVariableAccumulationDistribution(int windowSize)
            : base(1)
        {
            _ms = new MovingSum(windowSize);
        }

        public override double Update(Bar bar)
        {
            var index = (bar.ClosePrice - bar.OpenPrice) * bar.Volume / (bar.HighestPrice - bar.LowestPrice);

            return _ms.Update(index);
        }
    }
}
