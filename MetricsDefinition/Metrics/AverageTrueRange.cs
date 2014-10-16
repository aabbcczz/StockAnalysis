using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    /// <summary>
    /// The ATR metric
    /// </summary>
    [Metric("ATR")]
    public sealed class AverageTrueRange : SingleOutputBarInputSerialMetric
    {
        private double _prevClosePrice;
        private readonly MovingAverage _maTrueRange;
        public AverageTrueRange(int windowSize)
            : base(1)
        {
            _maTrueRange = new MovingAverage(windowSize);
        }

        public override double Update(Bar bar)
        {
            var trueRange = 
                Math.Max(
                    Math.Max(bar.HighestPrice - bar.LowestPrice, bar.HighestPrice - _prevClosePrice),
                    _prevClosePrice - bar.LowestPrice);

            _prevClosePrice = bar.ClosePrice;

            return _maTrueRange.Update(trueRange);
        }
    }
}
