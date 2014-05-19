using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("EMA, EXPMA")]
    class ExponentialMovingAverage : Metric
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

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input[0];
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
