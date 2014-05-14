using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("SD", "lookback:System.Int32")]
    public class StdDev : IGeneralMetric
    {
        private int _lookback;

        public StdDev(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback must be greater than zero");
            }

            _lookback = lookback;
        }

        public IEnumerable<double> Calculate(IEnumerable<double> input)
        {
            if (input == null || input.Count() == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input.ToArray();

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback - 1)
                {
                    yield return 0.0;
                }
                else
                {
                    double sum = 0.0;

                    for (int j = i - _lookback + 1; j <= i; ++j)
                    {
                        sum += allData[j];
                    }

                    double average = sum / _lookback;
                    double sumOfSquares = 0;

                    for (int j = i - _lookback + 1; j <= i; ++j)
                    {
                        sumOfSquares += (allData[j] - average) * (allData[j] - average);
                    }

                    yield return Math.Sqrt(sumOfSquares / _lookback);
                }
            }
        }
    }
}
