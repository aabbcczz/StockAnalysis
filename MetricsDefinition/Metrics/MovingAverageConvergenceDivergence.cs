using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MACD", "DIFF,DEA")]
    public sealed class MovingAverageConvergenceDivergence : MultipleOutputRawInputSerialMetric
    {
        private ExponentialMovingAverage _emaShort;
        private ExponentialMovingAverage _emaLong;
        private ExponentialMovingAverage _emaDiff;

        public MovingAverageConvergenceDivergence(int shortWindowSize, int longWindowSize, int diffWindowSize)
            : base(1)
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
            double emaShort = _emaShort.Update(dataPoint);
            double emaLong = _emaLong.Update(dataPoint);
            double diff = emaShort - emaLong;
            double dea = _emaDiff.Update(diff);

            return new double[2] { diff, dea };
        }
    }
}
