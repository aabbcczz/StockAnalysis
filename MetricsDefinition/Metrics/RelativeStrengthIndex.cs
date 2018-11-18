using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("RSI")]
    public sealed class RelativeStrengthIndex : SingleOutputRawInputSerialMetric
    {
        private readonly MovingSum _msUc;
        private readonly MovingSum _msDc;
        private double _prevData;
        private bool _firstData = true;
        public RelativeStrengthIndex(int windowSize)
            : base(0)
        {
            _msUc = new MovingSum(windowSize);
            _msDc = new MovingSum(windowSize);
        }

        public override void Update(double dataPoint)
        {
            var uc = _firstData
                ? 0.0
                : Math.Max(0.0, dataPoint - _prevData);

            var dc = _firstData
                ? 0.0
                : Math.Max(0.0, _prevData - dataPoint);
            
            _msUc.Update(uc);
            var msuc = _msUc.Value;

            _msDc.Update(dc);
            var msdc = _msDc.Value;

            // update status
            _prevData = dataPoint;
            _firstData = false;

            var rsi = (Math.Abs(msuc + msdc) < 1e-6) ? 0.0 : msuc / (msuc + msdc) * 100.0;

            SetValue(rsi);
        }
    }
}
