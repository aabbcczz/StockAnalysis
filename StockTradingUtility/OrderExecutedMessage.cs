using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.StockTrading.Utility
{
    public sealed class OrderExecutedMessage
    {
        public IOrder Order { get; set; }

        public float DealPrice { get; set; }

        public int DealVolume { get; set; }
    }
}
