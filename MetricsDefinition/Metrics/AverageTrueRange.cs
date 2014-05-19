using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using System.Reflection;

namespace MetricsDefinition
{
    /// <summary>
    /// The ATR metric
    /// </summary>
    [Metric("ATR")]
    public sealed class AverageTrueRange : Metric
    {
        private int _lookback;
        
        public AverageTrueRange(int lookback)
        {
            if (lookback <= 1)
            {
                throw new ArgumentException("lookback must be greater than 1");
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // ATR can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("ATR can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double prevCp = 0.0;
            double[] trueRange = new double[hp.Length];

            for (int i = 0; i < hp.Length; ++i)
            {
                trueRange[i] = 
                    Math.Max(
                        Math.Max(hp[i] - lp[i],
                            hp[i] - prevCp),
                        prevCp - lp[i]);

                prevCp = cp[i];
            }

            return new MovingAverage(_lookback).Calculate(new double[1][] { trueRange });
        }
    }
}
