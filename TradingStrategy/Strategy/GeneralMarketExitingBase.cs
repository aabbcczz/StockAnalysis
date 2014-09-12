using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralMarketExitingBase 
        : GeneralTradingStrategyComponentBase
        , IMarketExitingComponent
    {
        public abstract bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
