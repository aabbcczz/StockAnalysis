using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace StockTradingConsole
{
    public sealed class OldStock
    {
        public StockName Name { get; set; }
        public int Volume { get; set; }

        public OldStock()
        {
        }

        public OldStock(OldStockForSerialization oss)
        {
            Name = new StockName(oss.SecurityCode, oss.SecurityName);
            Volume = Volume;
        }
    }
}
