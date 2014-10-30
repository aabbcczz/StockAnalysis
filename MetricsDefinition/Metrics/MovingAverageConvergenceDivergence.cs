using System;

namespace MetricsDefinition.Metrics
{
    [Metric("MACD", "DIFF,DEA")]
    public sealed class MovingAverageConvergenceDivergence : MultipleOutputRawInputSerialMetric
    {
        private readonly ExponentialMovingAverage _emaShort;
        private readonly ExponentialMovingAverage _emaLong;
        private readonly ExponentialMovingAverage _emaDiff;

        public MovingAverageConvergenceDivergence(int shortWindowSize, int longWindowSize, int diffWindowSize)
            : base(0)
        {
            if (shortWindowSize < 1 || longWindowSize < 1 || diffWindowSize < 1)
            {
                throw new ArgumentOutOfRangeException("No any parameter can be smaller than 1");
            }

            if (shortWindowSize >= longWindowSize)
            {
                throw new ArgumentException("short windowSize should be smaller than long windowSize");
            }

            _emaShort = new ExponentialMovingAverage(shortWindowSize);
            _emaLong = new ExponentialMovingAverage(longWindowSize);
            _emaDiff = new ExponentialMovingAverage(diffWindowSize);
        }

        public override double[] Update(double dataPoint)
        {
            var emaShort = _emaShort.Update(dataPoint);
            var emaLong = _emaLong.Update(dataPoint);
            var diff = emaShort - emaLong;
            var dea = _emaDiff.Update(diff);

            return new[] { diff, dea };
        }
    }
}
