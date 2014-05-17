using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("EMA, EXPMA")]
    class ExponentialMovingAverage : IMetric
    {
        private int _lookback;

        public ExponentialMovingAverage(int lookback)
        {
            if (lookback < 2)
            {
                throw new ArgumentOutOfRangeException("lookback must be greater than 1");
            }

            _lookback = lookback;
        }

        public IEnumerable<double>[] Calculate(IEnumerable<double>[] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length != 1)
            {
                throw new ArgumentException("ExponentialMovingAverage accept only one field of input");
            }

            double[] allData = input[0].ToArray();
            double[] result = new double[allData.Length];

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i == 0)
                {
                    result[i] = allData[0];
                }
                else
                {
                    result[i] = (result[i - 1] * (_lookback - 1) + allData[i] * 2.0) / (_lookback + 1);
                }
            }

            return new double[1][] { result };
        }
    }
}
