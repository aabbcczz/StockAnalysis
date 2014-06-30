using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using System.Reflection;

namespace MetricsDefinition
{
    /// <summary>
    /// The ATR metric
    /// </summary>
    [Metric("ATR")]
    public sealed class AverageTrueRange : SingleOutputBarInputSerialMetric
    {
        private double _prevClosePrice = 0.0;
        private MovingAverage _maTrueRange;
        public AverageTrueRange(int windowSize)
            : base(1)
        {
            _maTrueRange = new MovingAverage(windowSize);
        }

        public override double Update(Bar bar)
        {
            double trueRange = 
                Math.Max(
                    Math.Max(bar.HighestPrice - bar.LowestPrice, bar.HighestPrice - _prevClosePrice),
                    _prevClosePrice - bar.LowestPrice);

            _prevClosePrice = bar.ClosePrice;

            return _maTrueRange.Update(trueRange);
        }
    }
}
