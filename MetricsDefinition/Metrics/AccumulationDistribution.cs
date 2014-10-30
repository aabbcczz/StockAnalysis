using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("AD")]
    public sealed class AccumulationDistribution : SingleOutputBarInputSerialMetric
    {
        private readonly MovingSum _sumCost;
        private readonly MovingSum _sumVolume;

        public AccumulationDistribution(int windowSize)
            : base(0)
        {
            _sumCost = new MovingSum(windowSize);
            _sumVolume = new MovingSum(windowSize);
        }

        public override double Update(Bar bar)
        {
            var cost = ((bar.ClosePrice - bar.LowestPrice) 
                - (bar.HighestPrice - bar.ClosePrice)) 
                / (bar.HighestPrice - bar.LowestPrice)
                * bar.Volume;

            return _sumCost.Update(cost) / _sumVolume.Update(bar.Volume);
        }
    }
}
