using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("W%R, WMS%R")]
    class WilliamOverboughtOversoldIndex : IMetric
    {
        private int _lookback;
        
        static private int HighestPriceFieldIndex;
        static private int LowestPriceFieldIndex;
        static private int ClosePriceFieldIndex;

        static WilliamOverboughtOversoldIndex()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            HighestPriceFieldIndex = attribute.NameToFieldIndexMap["HP"];
            LowestPriceFieldIndex = attribute.NameToFieldIndexMap["LP"];
            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
        }

        public WilliamOverboughtOversoldIndex(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // KDJ can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("KDJ can only accept StockData's output as input");
            }

            double[] highestPrices = input[HighestPriceFieldIndex];
            double[] lowestPrices = input[LowestPriceFieldIndex];
            double[] closePrices = input[ClosePriceFieldIndex];

            double lowestPrice = double.MaxValue;
            int lowestPriceIndex = -1;
            double highestPrice = double.MinValue;
            int highestPriceIndex = -1;

            double[] result = new double[closePrices.Length];

            for (int i = 0; i < closePrices.Length; ++i)
            {
                // find out the lowest price and highest price in past _kLookback period.
                if (lowestPrices[i] <= lowestPrice)
                {
                    lowestPrice = lowestPrices[i];
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
                            if (lowestPrices[m] <= lowestPrice)
                            {
                                lowestPrice = lowestPrices[m];
                                lowestPriceIndex = m;
                            }
                        }
                    }
                }

                if (highestPrices[i] >= highestPrice)
                {
                    highestPrice = highestPrices[i];
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
                            if (highestPrices[m] >= highestPrice)
                            {
                                highestPrice = highestPrices[m];
                                highestPriceIndex = m;
                            }
                        }
                    }
                }

                // calculate RSV
                double rsv = (closePrices[i] - lowestPrice) / (highestPrice - lowestPrice) * 100;
                result[i] = 100.0 - rsv;
            }

            return new double[1][] { result };
        }
    }
}
