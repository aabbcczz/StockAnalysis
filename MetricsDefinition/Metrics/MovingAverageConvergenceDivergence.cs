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

        public double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }


            double[] allData = input[0];
            double[] diff = new double[allData.Length];

            var emaShort = new ExponentialMovingAverage(_shortLookback)
                .Calculate(input)[0];

            var emaLong = new ExponentialMovingAverage(_longLookback)
                .Calculate(input)[0];

            for (int i = 0; i < diff.Length; ++i)
            {
                diff[i] = emaShort[i] - emaLong[i];
            }

            var dea = new ExponentialMovingAverage(_diffLookback)
                .Calculate(new double[1][] { diff })[0];

            return new double[2][] { diff, dea };
        }
    }
}
