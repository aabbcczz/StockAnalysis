using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("KDJ", "K,D,J")]
    public sealed class KDJStochastics : Metric
    {
        private int _kLookback;
        private int _kDecay;
        private int _jCoeff;
        
        public KDJStochastics(int kLookback, int kDecay, int jCoeff)
        {
            if (kLookback <= 0 || kDecay <= 0 || jCoeff <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _kLookback = kLookback;
            _kDecay = kDecay;
            _jCoeff = jCoeff;
        }

        public override double[][] Calculate(double[][] input)
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

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double lowestPrice = double.MaxValue;
            int lowestPriceIndex = -1;
            double highestPrice = double.MinValue;
            int highestPriceIndex = -1;
            double previousK = 50.0;
            double previousD = 50.0;

            double[] kResult = new double[cp.Length];
            double[] dResult = new double[cp.Length];
            double[] jResult = new double[cp.Length];

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
                    if (i >= _kLookback && lowestPriceIndex < i - _kLookback + 1)
                    {
                        lowestPrice = double.MaxValue;
                        lowestPriceIndex = -1;
                        for (int m = i - _kLookback + 1; m <= i; ++m)
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
                    if (i >= _kLookback && highestPriceIndex < i - _kLookback + 1)
                    {
                        highestPrice = double.MinValue;
                        highestPriceIndex = -1;
                        for (int m = i - _kLookback + 1; m <= i; ++m)
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

                kResult[i] = ((_kDecay - 1) * previousK + rsv) / _kDecay;
                previousK = kResult[i];

                dResult[i] = ((_kDecay - 1) * previousD + kResult[i]) / _kDecay;
                previousD = dResult[i];

                jResult[i] = _jCoeff * dResult[i] - (_jCoeff - 1) * kResult[i];

            }

            return new double[3][] { kResult, dResult, jResult };
        }
    }
}
