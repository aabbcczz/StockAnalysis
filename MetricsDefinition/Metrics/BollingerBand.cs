using System;

namespace MetricsDefinition.Metrics
{
    [Metric("BOLL", "UB,MA,LB")]
    public sealed class BollingerBand : MultipleOutputRawInputSerialMetric
    {
        private readonly double _alpha;

        private readonly MovingAverage _ma;
        private readonly StdDev _sd;

        public BollingerBand(int windowSize, double alpha)
            : base(1)
        {
            if (alpha <= 0.0)
            {
                throw new ArgumentOutOfRangeException("alpha");
            }

            _alpha = alpha;

            _ma = new MovingAverage(windowSize);
            _sd = new StdDev(windowSize);
        }

        public override double[] Update(double dataPoint)
        {
            var ma = _ma.Update(dataPoint);
            var stddev = _sd.Update(dataPoint);

            var upperBound = ma + _alpha * stddev;
            var lowerBound = ma - _alpha * stddev;

            return new[] { upperBound, ma, lowerBound };
        }
    }
}
