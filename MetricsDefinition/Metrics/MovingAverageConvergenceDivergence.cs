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

            Values = new double[2];
        }

        public override void Update(double dataPoint)
        {
            _emaShort.Update(dataPoint);
            var emaShort = _emaShort.Value;
            
            _emaLong.Update(dataPoint);
            var emaLong = _emaLong.Value;

            var diff = emaShort - emaLong;

            _emaDiff.Update(diff);
            var dea = _emaDiff.Value;

            SetValue(diff, dea);
        }
    }
}
