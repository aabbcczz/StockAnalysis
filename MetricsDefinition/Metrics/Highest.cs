using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("HI")]
    public sealed class Highest : Metric
    {
        private int _lookback;

        public Highest(int lookback)
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

            double highestPrice = double.MinValue;
            int highestPriceIndex = -1;

            double[] data = input[0];
            double[] result = new double[data.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                if (data[i] >= highestPrice)
                {
                    highestPrice = data[i];
                    highestPriceIndex = i;
                }
                else
                {
                    // determine if current highest price is still valid
                    if (i >= _lookback && highestPriceIndex < i - _lookback + 1)
                    {
                        highestPrice = double.MinValue;
                        highestPriceIndex = -1;
                        for (int m = i - _lookback + 1; m <= i; ++m)
                        {
                            if (data[m] >= highestPrice)
                            {
                                highestPrice = data[m];
                                highestPriceIndex = m;
                            }
                        }
                    }
                }

                result[i] = highestPrice;

            }

            return new double[1][] { result };
        }
    }
}
