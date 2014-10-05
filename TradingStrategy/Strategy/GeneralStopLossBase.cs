using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralStopLossBase 
        : GeneralTradingStrategyComponentBase
        , IStopLossComponent
    {
        public abstract double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments);
    }
}
