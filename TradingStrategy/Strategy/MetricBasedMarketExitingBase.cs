using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedMarketExitingBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IMarketExitingComponent
        where T : IRuntimeMetric
    {
        public abstract bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
