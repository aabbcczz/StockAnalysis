using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Base
{
    public abstract class GeneralBuyPriceFilteringBase : 
        GeneralTradingStrategyComponentBase, 
        IBuyPriceFilteringComponent
    {
        public abstract bool IsPriceAcceptable(ITradingObject tradingObject, double price, out string comments);
    }
}
