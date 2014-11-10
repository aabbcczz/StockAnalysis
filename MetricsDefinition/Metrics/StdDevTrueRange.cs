using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    /// <summary>
    /// The StdDev of TureRange metric
    /// </summary>
    [Metric("SDTR")]
    public sealed class StdDevTrueRange : SingleOutputBarInputSerialMetric
    {
        private double _prevClosePrice;
        private readonly StdDev _sdTrueRange;
        public StdDevTrueRange(int windowSize)
            : base(0)
        {
            _sdTrueRange = new StdDev(windowSize);
        }

        public override void Update(Bar bar)
        {
            var trueRange = 
                Math.Max(
                    Math.Max(bar.HighestPrice - bar.LowestPrice, bar.HighestPrice - _prevClosePrice),
                    _prevClosePrice - bar.LowestPrice);

            _prevClosePrice = bar.ClosePrice;

            _sdTrueRange.Update(trueRange);

            var sdtr = _sdTrueRange.Value;
            SetValue(sdtr);
        }
    }
}
