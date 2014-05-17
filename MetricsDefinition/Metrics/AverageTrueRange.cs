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
    public sealed class AverageTrueRange : IMetric
    {
        private int _lookback;
        
        static private int _highestPriceFieldIndex;
        static private int _lowestPriceFieldIndex;
        static private int _closePriceFieldIndex;


        static AverageTrueRange()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            _highestPriceFieldIndex = attribute.NameToFieldIndexMap["HP"];
            _lowestPriceFieldIndex = attribute.NameToFieldIndexMap["LP"];
            _closePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
        }

        public AverageTrueRange(int lookback)
        {
            if (lookback <= 1)
            {
                throw new ArgumentException("days must be greater than 1");
            }

            _lookback = lookback;
        }

        public IEnumerable<double>[] Calculate(IEnumerable<double>[] input)
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

            double[] highestPrice = input[_highestPriceFieldIndex].ToArray();
            double[] lowestPrice = input[_lowestPriceFieldIndex].ToArray();
            double[] closePrice = input[_closePriceFieldIndex].ToArray();

            double previousDayClosePrice = 0.0;
            double[] trueRange = new double[highestPrice.Length];

            for (int i = 0; i < highestPrice.Length; ++i)
            {
                trueRange[i] = 
                    Math.Max(
                        Math.Max(highestPrice[i] - lowestPrice[i],
                            highestPrice[i] - previousDayClosePrice),
                        previousDayClosePrice - lowestPrice[i]);

                previousDayClosePrice = closePrice[i];
            }

            double[] result = new double[trueRange.Length];

            double previousDayAverageTrueRange = 0.0;

            for (int i = 0; i < trueRange.Length; ++i)
            {
                if (i < _lookback)
                {
                    previousDayAverageTrueRange = trueRange.Take(i + 1).Average();
                    result[i] = previousDayAverageTrueRange;
                }
                else
                {
                    previousDayAverageTrueRange = ((_lookback - 1) * previousDayAverageTrueRange + trueRange[i] ) / _lookback;
                    result[i] = previousDayAverageTrueRange;
                }
            }

            return new double[1][] { result };
        }
    }
}
