using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    /// <summary>
    /// The ATR metric
    /// </summary>
    [Metric("ATR")]
    public sealed class AverageTrueRange : IStockTransactionSummaryMetric
    {
        private int _days;

        public AverageTrueRange(int days)
        {
            if (days <= 1)
            {
                throw new ArgumentException("days must be greater than 1");
            }

            _days = days;
        }

        public IEnumerable<double> Calculate(IEnumerable<StockTransactionSummary> input)
        {
 	        if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            double previousDayClosePrice = 0.0;
            List<double> trueRange = new List<double>(input.Count());

            foreach (var data in input)
            {
                trueRange.Add(
                    Math.Max(
                        Math.Max(data.HighestPrice - data.LowestPrice, 
                            data.HighestPrice - previousDayClosePrice), 
                        previousDayClosePrice - data.LowestPrice));

                previousDayClosePrice = data.ClosePrice;
            }

            double previousDayAverageTrueRange = 0.0;

            for (int i = 0; i < trueRange.Count; ++i)
            {
                if (i < _days - 1)
                {
                    yield return 0.0;
                }
                else if (i == _days - 1)
                {
                    previousDayAverageTrueRange = trueRange.Take(_days).Average();
                    yield return previousDayAverageTrueRange;
                }
                else
                {
                    previousDayAverageTrueRange = ((_days - 1) * previousDayAverageTrueRange + trueRange[i] ) / _days;

                    yield return previousDayAverageTrueRange;
                }
            }
        }
    }
}
