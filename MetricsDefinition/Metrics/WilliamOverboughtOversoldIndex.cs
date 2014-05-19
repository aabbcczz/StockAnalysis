using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("W%R, WMS%R")]
    class WilliamOverboughtOversoldIndex : Metric
    {
        private int _lookback;
        
        public WilliamOverboughtOversoldIndex(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // W%R can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("W%R can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double lowestPrice = double.MaxValue;
            int lowestPriceIndex = -1;
            double highestPrice = double.MinValue;
            int highestPriceIndex = -1;

            double[] result = new double[cp.Length];

            for (int i = 0; i < cp.Length; ++i)
            {
                // find out the lowest price and highest price in past _kLookback period.
                if (lp[i] <= lowestPrice)
                {
                    lowestPrice = lp[i];
                    lowestPriceIndex = i;
                }
                else
                {
                    // determine if current lowestPrice is still valid
                    if (i >= _lookback && lowestPriceIndex < i - _lookback + 1)
                    {
                        lowestPrice = double.MaxValue;
                        lowestPriceIndex = -1;
                        for (int m = i - _lookback + 1; m <= i; ++m)
                        {
                            if (lp[m] <= lowestPrice)
                            {
                                lowestPrice = lp[m];
                                lowestPriceIndex = m;
                            }
                        }
                    }
                }

                if (hp[i] >= highestPrice)
                {
                    highestPrice = hp[i];
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
                            if (hp[m] >= highestPrice)
                            {
                                highestPrice = hp[m];
                                highestPriceIndex = m;
                            }
                        }
                    }
                }

                // calculate RSV
                double rsv = (cp[i] - lowestPrice) / (highestPrice - lowestPrice) * 100;
                result[i] = 100.0 - rsv;
            }

            return new double[1][] { result };
        }
    }
}
