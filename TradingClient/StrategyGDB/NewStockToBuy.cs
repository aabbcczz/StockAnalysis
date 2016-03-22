using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingClient.StrategyGDB
{
    public sealed class NewStockToBuy
    {
        public DateTime DateToBuy { get; set; }
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public float OpenPriceUpLimit { get; set; }
        public float OpenPriceDownLimit { get; set; }
        public float MaxBuyPriceIncreasePercentage { get; set; }
        public float TotalCapital { get; set; }
    }
}
