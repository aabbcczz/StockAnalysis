using System;
using StockAnalysis.Common.Data;

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
            : base(0)
        {
            _maTrueRange = new MovingAverage(windowSize);
        }

        public override void Update(Bar bar)
        {
            var trueRange = 
                Math.Max(
                    Math.Max(bar.HighestPrice - bar.LowestPrice, bar.HighestPrice - _prevClosePrice),
                    _prevClosePrice - bar.LowestPrice);

            _prevClosePrice = bar.ClosePrice;

            _maTrueRange.Update(trueRange);

            SetValue(_maTrueRange.Value);
        }
    }
}
