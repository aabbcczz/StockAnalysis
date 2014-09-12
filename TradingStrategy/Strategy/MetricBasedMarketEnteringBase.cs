using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedMarketEnteringBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IMarketEnteringComponent
        where T : IRuntimeMetric
    {
        public abstract bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
