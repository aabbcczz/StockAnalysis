using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MTM")]
    class Momentum : Metric
    {
         private int _lookback;

         public Momentum(int lookback)
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

            double[] allData = input[0];
            double[] result = new double[allData.Length];

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback)
                {
                    result[i] = 0.0;
                }
                else
                {
                    result[i] = allData[i] * 100.0 / allData[i - _lookback];
                }
            }

            return new double[1][] { result };
        }
    }
}
