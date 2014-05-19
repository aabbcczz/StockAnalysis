using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MACD", "DIFF,DEA")]
    public class MovingAverageConvergenceDivergence : Metric
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

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }


            var emaShort = new ExponentialMovingAverage(_shortLookback)
                .Calculate(input[0]);

            var emaLong = new ExponentialMovingAverage(_longLookback)
                .Calculate(input[0]);

            var diff = emaShort.OperateThis(emaLong, (s, l) => s - l);

            var dea = new ExponentialMovingAverage(_diffLookback)
                .Calculate(diff);

            return new double[2][] { diff, dea };
        }
    }
}
