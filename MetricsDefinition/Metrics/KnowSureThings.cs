using System;

namespace MetricsDefinition.Metrics
{
    [Metric("KST")]
    public sealed class KnowSureThings : SingleOutputRawInputSerialMetric
    {
        private readonly RateOfChange _roc1;
        private readonly RateOfChange _roc2;
        private readonly RateOfChange _roc3;
        private readonly RateOfChange _roc4;

        public KnowSureThings(int windowSize1, int windowSize2, int windowSize3, int windowSize4)
            : base(0)
        {
            if (windowSize1 <= 0 || windowSize2 <= 0 || windowSize3 <= 0 || windowSize4 <= 0)
            {
                throw new ArgumentOutOfRangeException("windowSize");
            }

            _roc1 = new RateOfChange(windowSize1);
            _roc2 = new RateOfChange(windowSize2);
            _roc3 = new RateOfChange(windowSize3);
            _roc4 = new RateOfChange(windowSize4);
        }

        public override void Update(double dataPoint)
        {
            _roc1.Update(dataPoint);
            _roc2.Update(dataPoint);
            _roc3.Update(dataPoint);
            _roc4.Update(dataPoint);

            var roc1 = _roc1.Value;
            var roc2 = _roc2.Value;
            var roc3 = _roc3.Value;
            var roc4 = _roc4.Value;

            var kst = roc1 + 2 * roc2 + 3 * roc3 + 4 * roc4 / (1 + 2 + 3 + 4);
            SetValue(kst);
        } 
    }
}
