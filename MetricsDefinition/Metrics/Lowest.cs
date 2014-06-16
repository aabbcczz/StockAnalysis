using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("LO")]
    public sealed class Lowest : Metric
    {
        private int _lookback;

        public Lowest(int lookback)
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

            double lowestPrice = double.MaxValue;
            int lowestPriceIndex = -1;

            double[] data = input[0];
            double[] result = new double[data.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                if (data[i] <= lowestPrice)
                {
                    lowestPrice = data[i];
                    lowestPriceIndex = i;
                }
                else
                {
                    // determine if current highest price is still valid
                    if (i >= _lookback && lowestPriceIndex < i - _lookback + 1)
                    {
                        lowestPrice = double.MaxValue;
                        lowestPriceIndex = -1;
                        for (int m = i - _lookback + 1; m <= i; ++m)
                        {
                            if (data[m] <= lowestPrice)
                            {
                                lowestPrice = data[m];
                                lowestPriceIndex = m;
                            }
                        }
                    }
                }

                result[i] = lowestPrice;

            }

            return new double[1][] { result };
        }
    }
}
