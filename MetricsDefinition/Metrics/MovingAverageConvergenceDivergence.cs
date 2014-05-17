using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition.Metrics
{
    [Metric("MACD", "DIFF,DEA")]
    public class MovingAverageConvergenceDivergence : IMetric
    {
        private int _shortLookback;
        private int _longLookback;
        private int _diffLookback;

        public MovingAverageConvergenceDivergence(int shortLookback, int longLookback, int diffLookback)
        {
            if (shortLookback < 1 || longLookback < 1 || diffLookback < 1)
            {
                throw new ArgumentOutOfRangeException("No any parameter can be smaller than 1");
            }

            if (shortLookback >= longLookback)
            {
                throw new ArgumentException("short lookback should be smaller than long lookback");
            }

            _shortLookback = shortLookback;
            _longLookback = longLookback;
            _diffLookback = diffLookback;
        }

        public IEnumerable<double>[] Calculate(IEnumerable<double>[] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length != 1)
            {
                throw new ArgumentException("MACD accept only one field of input");
            }


            double[] allData = input[0].ToArray();
            double[] diff = new double[allData.Length];

            var emaShort = new ExponentialMovingAverage(_shortLookback)
                .Calculate(input)
                .First()
                .ToArray();

            var emaLong = new ExponentialMovingAverage(_longLookback)
                .Calculate(input)
                .First()
                .ToArray();

            for (int i = 0; i < diff.Length; ++i)
            {
                diff[i] = emaShort[i] - emaLong[i];
            }

            var dea = new ExponentialMovingAverage(_diffLookback)
                .Calculate(new IEnumerable<double>[1] { diff })
                .First()
                .ToArray();

            return new double[2][] { diff, dea };
        }
    }
}
