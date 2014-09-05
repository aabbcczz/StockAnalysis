using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public class MetricBasedTradingStrategyComponentBase<T>
        where T : IRuntimeMetric
    {
        protected RuntimeMetricManager<T> MetricManager { get; private set; }

        protected void Initialize(Func<T> creator)
        {
            MetricManager = new RuntimeMetricManager<T>(creator);
        }

        protected void WarmUp(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            MetricManager.Update(tradingObject, bar);
        }

        protected void Evaluate(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            MetricManager.Update(tradingObject, bar);
        }

        protected void Finish()
        {
            MetricManager = null;
        }
    }
}
