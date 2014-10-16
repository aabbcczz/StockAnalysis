using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{

    [Metric("COSTMA,CYC,CMA")]
    public sealed class CostMovingAverage : SingleOutputBarInputSerialMetric
    {
        private readonly MovingSum _msCost;
        private readonly MovingSum _msVolume;

        public CostMovingAverage(int windowSize)
            : base (windowSize)
        {
            _msCost = new MovingSum(windowSize);
            _msVolume = new MovingSum(windowSize);
        }

        public override double Update(Bar bar)
        {
            var truePrice = (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice ) / 4;

            var sumCost = _msCost.Update(bar.Volume * truePrice);
            var sumVolume = _msVolume.Update(bar.Volume);

            return sumCost / sumVolume;
        }
    }
}
