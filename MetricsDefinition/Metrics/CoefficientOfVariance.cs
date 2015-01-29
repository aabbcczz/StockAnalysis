using System;

namespace MetricsDefinition.Metrics
{
    [Metric("COV")]
    public sealed class CoefficientOfVariance : SingleOutputRawInputSerialMetric
    {
        private double _previousData = 0.0;
        private StdDev _stddev;
        private MovingAverage _movingAverage;

        public CoefficientOfVariance(int windowSize)
            : base(0)
        {
            if (windowSize <= 1)
            {
                throw new ArgumentException("window size must be greater than 1");
            }

            _stddev = new StdDev(windowSize);
            _movingAverage = new MovingAverage(windowSize);
        }

        public override void Update(double dataPoint)
        {
            if (_previousData != 0.0)
            {
                //var gain = dataPoint;
                var profitRatio = (dataPoint - _previousData) / _previousData;
                _stddev.Update(profitRatio);
                _movingAverage.Update(profitRatio);

                if (Math.Abs(_movingAverage.Value) < 1e-6)
                {
                    SetValue(_stddev.Value / 0.0001);
                }
                else
                {
                    SetValue(_stddev.Value / Math.Abs(_movingAverage.Value));
                }
            }

            _previousData = dataPoint;
        }
     }
}
