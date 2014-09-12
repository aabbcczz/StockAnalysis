using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedStopLossBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IStopLossComponent
        where T : IRuntimeMetric
    {
        public abstract double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice);
    }
}
