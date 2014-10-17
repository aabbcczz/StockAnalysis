using System;

namespace MetricsDefinition.Metrics
{
    [Metric("RSI")]
    public sealed class RelativeStrengthIndex : SingleOutputRawInputSerialMetric
    {
        private readonly MovingSum _msUc;
        private readonly MovingSum _msDc;
        private double _prevData;
        private bool _firstData = true;
        public RelativeStrengthIndex(int windowSize)
            : base(1)
        {
            _msUc = new MovingSum(windowSize);
            _msDc = new MovingSum(windowSize);
        }

        public override double Update(double dataPoint)
        {
            var uc = _firstData
                ? 0.0
                : Math.Max(0.0, dataPoint - _prevData);

            var dc = _firstData
                ? 0.0
                : Math.Max(0.0, _prevData - dataPoint);

            var msuc = _msUc.Update(uc);
            var msdc = _msDc.Update(dc);

            // update status
            _prevData = dataPoint;
            _firstData = false;

            return (Math.Abs(msuc + msdc) < 1e-6) ? 0.0 : msuc / (msuc + msdc) * 100.0;
        }
    }
}
