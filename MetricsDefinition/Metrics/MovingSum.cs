using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MS")]
    public sealed class MovingSum : Metric
    {
        private int _lookback;

        public MovingSum(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double sum = 0.0;

            double[] allData = input[0];
            double[] result = new double[allData.Length];

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback)
                {
                    sum += allData[i];
                    result[i] = sum;
                }
                else
                {
                    sum = sum - allData[i - _lookback] + allData[i];
                    result[i] = sum;
                }
            }

            return new double[1][] { result };
        }
    }
}
