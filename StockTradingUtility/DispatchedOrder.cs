using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class DispatchedOrder
    {
        public DateTime DispatchedTime { get; set; }

        public int OrderNo { get; private set; }

        public int LastTotalDealVolume { get; set; }

        public float LastAverageDealPrice { get; set; }

        public int LastDeltaDealVolume { get; set; }

        public OrderStatus LastStatus { get; set; }

        public OrderRequest Request { get; private set; }

        public Action<DispatchedOrder> OnOrderStatusChanged { get; private set; }

        public DispatchedOrder(OrderRequest request, Action<DispatchedOrder> onOrderStatusChanged, int orderNo)
        {
            Request = request;
            OnOrderStatusChanged = onOrderStatusChanged;
            OrderNo = orderNo;

            DispatchedTime = DateTime.Now;
            LastAverageDealPrice = 0.0f;
            LastTotalDealVolume = 0;
            LastStatus = OrderStatus.NotSubmitted;

        }
    }
}
