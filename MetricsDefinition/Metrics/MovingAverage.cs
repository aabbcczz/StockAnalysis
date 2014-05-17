using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MA")]
    public sealed class MovingAverage : IMetric
    {
        private int _lookback;

        public MovingAverage(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
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
                throw new ArgumentException("MovingAverage accept only one field of input");
            } 
            
            double sum = 0.0;

            double[] allData = input[0].ToArray();
            double[] result = new double[allData.Length];

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback)
                {
                    sum += allData[i];
                    result[i] = sum / (i + 1);
                }
                else
                {
                    sum = sum - allData[i - _lookback] + allData[i];
                    result[i] = sum / _lookback;
                }
            }

            return new double[1][] { result };
        }
    }
}
