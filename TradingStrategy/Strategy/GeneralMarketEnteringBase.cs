using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralMarketEnteringBase 
        : GeneralTradingStrategyComponentBase
        , IMarketEnteringComponent
    {
        public abstract bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
