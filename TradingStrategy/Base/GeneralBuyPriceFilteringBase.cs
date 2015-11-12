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
        public abstract BuyPriceFilteringComponentResult IsPriceAcceptable(ITradingObject tradingObject, double price);
    }
}
