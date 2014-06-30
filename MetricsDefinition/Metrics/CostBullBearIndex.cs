using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("CBBI")]
    public sealed class CostBullBearIndex : SingleOutputBarInputSerialMetric
    {
        private CostMovingAverage _cma1;
        private CostMovingAverage _cma2;
        private CostMovingAverage _cma3;
        private CostMovingAverage _cma4;

        public CostBullBearIndex(int windowSize1, int windowSize2, int windowSize3, int windowSize4)
            : base(1)
        {
            if (windowSize1 <= 0 || windowSize2 <= 0 || windowSize3 <= 0 || windowSize4 <=0)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than 0");
            }

            _cma1 = new CostMovingAverage(windowSize1);
            _cma2 = new CostMovingAverage(windowSize2);
            _cma3 = new CostMovingAverage(windowSize3);
            _cma4 = new CostMovingAverage(windowSize4);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            return (_cma1.Update(bar) +
                _cma2.Update(bar) +
                _cma3.Update(bar) +
                _cma4.Update(bar)) / 4;
        }
    }
}
