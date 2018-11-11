using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradingConsole
{
    public sealed class NewStockForSerialization
    {
        public DateTime DateToBuy { get; set; }
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public float BuyPriceUpLimitInclusive { get; set; }
        public float BuyPriceDownLimitInclusive { get; set; }
        public float TotalCapitalUsedToBuy { get; set; }

        public NewStockForSerialization()
        {
        }

        public NewStockForSerialization(NewStock ns)
        {
            DateToBuy = ns.DateToBuy;
            SecurityCode = ns.Name.NormalizedCode;
            SecurityName = ns.Name.Names[0];
            BuyPriceUpLimitInclusive = ns.BuyPriceUpLimitInclusive;
            BuyPriceDownLimitInclusive = ns.BuyPriceDownLimitInclusive;
            TotalCapitalUsedToBuy = ns.TotalCapitalUsedToBuy;
        }
    }
}
