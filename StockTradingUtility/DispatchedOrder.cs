using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class DispatchedOrder
    {
        public DateTime DispatchTime { get; set; }

        public int OrderNo { get; set; }

        public int LastDealVolume { get; set; }

        public float LastDealPrice { get; set; }

        public OrderStatus LastStatus { get; set; }

        public OrderRequest Request { get; set; }
    }
}
