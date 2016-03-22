using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingClient.StrategyGDB
{
    public sealed class ExistingStockToMaintain
    {
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public int HoldDays { get; set; }
        public float StoplossPrice { get; set; }
        public int Volume { get; set; }
    }
}
