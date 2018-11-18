using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("EXPRISK")]
    public sealed class ExpectedRisk : SingleOutputRawInputSerialMetric
    {
        private double _previousData = 0.0;
        private StdDev _stddev;

        public ExpectedRisk(int windowSize)
            : base(0)
        {
            if (windowSize <= 1)
            {
                throw new ArgumentException("window size must be greater than 1");
            }

            _stddev = new StdDev(windowSize);
        }

        public override void Update(double dataPoint)
        {
            if (_previousData != 0.0)
            {
                //var gain = dataPoint;
                var profitRatio = (dataPoint - _previousData) / _previousData;
                _stddev.Update(profitRatio);

                SetValue(_stddev.Value);
            }

            _previousData = dataPoint;
        }
     }
}
