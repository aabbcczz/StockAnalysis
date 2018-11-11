using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingClient.StrategyGDB
{
    public sealed class NewStock
    {
        public DateTime DateToBuy { get; set; }
        public string SecuritySymbol { get; set; }
        public string SecurityName { get; set; }
        public float StoplossPrice { get; set; }
        public float ActualOpenPrice { get; set; }
        public float OpenPriceUpLimitPercentage { get; set; }
        public float OpenPriceDownLimitPercentage { get; set; }
        public float ActualOpenPriceUpLimit { get; set; }
        public float ActualOpenPriceDownLimit { get; set; }
        public float MaxBuyPriceIncreasePercentage { get; set; }
        public float ActualMaxBuyPrice { get; set; }
        public float TodayDownLimitPrice { get; set; }
        public float ActualMinBuyPrice { get; set; }
        public float TotalCapital { get; set; }
        public bool IsBuyable { get; set; }
    }
}
