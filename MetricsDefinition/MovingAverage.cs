using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MA", "lookback:System.Int32")]
    public class MovingAverage : IGeneralMetric
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

        public IEnumerable<double> Calculate(IEnumerable<double> input)
        {
            double sum = 0.0;

            double[] allData = input.ToArray();

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback - 1)
                {
                    yield return 0.0;
                }
                else if (i == _lookback - 1)
                {
                    sum = input.Take(_lookback).Sum();
                    yield return sum / _lookback;
                }
                else
                {
                    sum = sum - allData[i - _lookback] + allData[i];
                    yield return sum / _lookback;
                }
            }
        }
    }
}
