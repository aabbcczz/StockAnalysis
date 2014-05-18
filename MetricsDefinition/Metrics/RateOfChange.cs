using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("ROC")]
    public sealed class RateOfChange : IMetric
    {
        private int _lookback;

        public RateOfChange(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input[0];
            double[] result = new double[allData.Length];

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback)
                {
                    result[i] = (allData[i] - allData[0]) / allData[0] * 100.0;
                }
                else
                {
                    result[i] = (allData[i] - allData[i - _lookback]) / allData[i - _lookback] * 100.0;
                }
            }

            return new double[1][] { result };
        }
    }
}
