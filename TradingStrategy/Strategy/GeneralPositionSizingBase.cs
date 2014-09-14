using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralPositionSizingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionSizingComponent
    {
        public abstract int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap);
    }
}
