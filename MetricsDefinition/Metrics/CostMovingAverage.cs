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

        public override void Update(Bar bar)
        {
            var truePrice = (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice ) / 4;
            
            _msCost.Update(bar.Volume * truePrice);
            var sumCost = _msCost.Value;
            
            _msVolume.Update(bar.Volume);
            var sumVolume = _msVolume.Value; 

            SetValue(sumCost / sumVolume);
        }
    }
}
