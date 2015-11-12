using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("CBBI")]
    public sealed class CostBullBearIndex : SingleOutputBarInputSerialMetric
    {
        private readonly CostMovingAverage _cma1;
        private readonly CostMovingAverage _cma2;
        private readonly CostMovingAverage _cma3;
        private readonly CostMovingAverage _cma4;

        public CostBullBearIndex(int windowSize1, int windowSize2, int windowSize3, int windowSize4)
            : base(0)
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

        public override void Update(Bar bar)
        {
            _cma1.Update(bar);
            _cma2.Update(bar);
            _cma3.Update(bar);
            _cma4.Update(bar);

            var cbbi = (_cma1.Value + _cma2.Value + _cma3.Value + _cma4.Value) / 4.0;
            SetValue(cbbi);
        }
    }
}
