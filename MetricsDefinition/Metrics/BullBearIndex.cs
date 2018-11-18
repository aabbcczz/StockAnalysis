using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("BBI")]
    public sealed class BullBearIndex : SingleOutputRawInputSerialMetric
    {
        private readonly MovingAverage _ma1;
        private readonly MovingAverage _ma2;
        private readonly MovingAverage _ma3;
        private readonly MovingAverage _ma4;

        public BullBearIndex(int windowSize1, int windowSize2, int windowSize3, int windowSize4)
            : base(0)
        {
            if (windowSize1 <= 0 || windowSize2 <= 0 || windowSize3 <= 0 || windowSize4 <=0)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than 0");
            }

            _ma1 = new MovingAverage(windowSize1);
            _ma2 = new MovingAverage(windowSize2);
            _ma3 = new MovingAverage(windowSize3);
            _ma4 = new MovingAverage(windowSize4);
        }

        public override void Update(double dataPoint)
        {
            _ma1.Update(dataPoint);
            _ma2.Update(dataPoint);
            _ma3.Update(dataPoint);
            _ma4.Update(dataPoint);

            var bbi = (_ma1.Value + _ma2.Value + _ma3.Value + _ma4.Value) / 4.0;

            SetValue(bbi);
        }
    }
}
