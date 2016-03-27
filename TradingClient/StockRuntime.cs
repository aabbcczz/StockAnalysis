using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockTrading.Utility;

namespace TradingClient
{
    sealed class StockRuntime
    {
        public string SecurityCode { get; private set; }

        public string SecurityName { get; private set; }

        public BuyOrder AssociatedBuyOrder { get; set; }

        public SellOrder AssociatedSellOrder { get; set; }
    }
}
