using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("AOM")]
    public sealed class AmountOfMoney : IStockTransactionSummaryMetric
    {
        public IEnumerable<double> Calculate(IEnumerable<StockAnalysis.Share.StockTransactionSummary> input)
        {
            return input.Select(s => s.AmountOfMoney);
        }
    }
}
