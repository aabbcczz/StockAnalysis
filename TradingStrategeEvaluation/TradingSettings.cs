using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;

namespace TradingStrategyEvaluation
{
    [Serializable]
    public sealed class TradingSettings
    {
        public CommissionSettings BuyingCommission { get; set; }

        public CommissionSettings SellingCommission { get; set; }

        public int Spread { get; set; }

//        public TradingPriceOption BuyShortPriceOption { get; set; }
//        public TradingPriceOption CloseShortPriceOption { get; set; }

        public TradingPriceOption OpenLongPriceOption { get; set; }
        public TradingPriceOption CloseLongPriceOption { get; set; }


    }
}
