using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Base
{
    public sealed class BuyPriceFilteringComponentResult : TradingStrategyComponentResult
    {
        public bool IsPriceAcceptable { get; set; }

        public BuyPriceFilteringComponentResult()
            : base()
        {
            IsPriceAcceptable = true;
        }
    }

    public interface IBuyPriceFilteringComponent : ITradingStrategyComponent
    {
        /// <summary>
        /// Decide if the buy price acceptable for given trading object
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="price">buy price</param>
        /// <returns>true if the price is acceptable, otherwise false is returned</returns>
        BuyPriceFilteringComponentResult IsPriceAcceptable(ITradingObject tradingObject, double price);
    }
}
