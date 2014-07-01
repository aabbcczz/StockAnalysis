using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public sealed class TradingSettings
    {
        public Commission BuyingCommission { get; set; }

        public Commission SellingCommission { get; set; }

        public int Spread { get; set; }

        public TradingPriceOption BuyShortPriceOption { get; set; }
        public TradingPriceOption BuyLongPriceOption { get; set; }
        public TradingPriceOption CloseShortPriceOption { get; set; }
        public TradingPriceOption CloseShortPriceOption { get; set; }

    }
}
