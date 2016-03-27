using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockTrading.Utility;

namespace TradingClient
{
    sealed class StockOrderRuntime
    {
        public string SecurityCode { get; private set; }

        public string SecurityName { get; private set; }

        public int ExpectedVolume { get; set; }

        public int RemainingVolume { get; set; }

        public BuyOrder AssociatedBuyOrder { get; set; }

        public SellOrder AssociatedSellOrder { get; set; }

        public StoplossOrder AssociatedStoplossOrder { get; set; }

        public StockOrderRuntime(string code, string name)
        {
            SecurityCode = code;
            SecurityName = name;
        }
    }
}
