using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("CRSI")]
    public sealed class CumulativeRelativeStrengthIndex : SingleOutputRawInputSerialMetric
    {
        private readonly MovingSum _msCrsi;
        private readonly RelativeStrengthIndex _rsi;

        public CumulativeRelativeStrengthIndex(int rsiWindowSize, int cumulativeWindowSize)
            : base(0)
        {
            _rsi = new RelativeStrengthIndex(rsiWindowSize);
            _msCrsi = new MovingSum(cumulativeWindowSize);
        }

        public override void Update(double dataPoint)
        {
            _rsi.Update(dataPoint);
            _msCrsi.Update(_rsi.Value);

            SetValue(_msCrsi.Value);
        }
    }
}
