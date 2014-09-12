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
    /// The StdDev of TureRange metric
    /// </summary>
    [Metric("SDTR")]
    public sealed class StdDevTrueRange : SingleOutputBarInputSerialMetric
    {
        private double _prevClosePrice = 0.0;
        private StdDev _sdTrueRange;
        public StdDevTrueRange(int windowSize)
            : base(1)
        {
            _sdTrueRange = new StdDev(windowSize);
        }

        public override double Update(Bar bar)
        {
            double trueRange = 
                Math.Max(
                    Math.Max(bar.HighestPrice - bar.LowestPrice, bar.HighestPrice - _prevClosePrice),
                    _prevClosePrice - bar.LowestPrice);

            _prevClosePrice = bar.ClosePrice;

            return _sdTrueRange.Update(trueRange);
        }
    }
}
