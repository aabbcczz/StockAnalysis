using System;
using StockAnalysis.Common.Utility;

namespace StockAnalysis.StockTrading.Utility
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

        public WaitableConcurrentQueue<OrderStatusChangedMessage> OrderStatusChangedMessageReceiver { get; private set; }

        private DispatchedOrder()
        {

        }

        public DispatchedOrder(OrderRequest request, WaitableConcurrentQueue<OrderStatusChangedMessage> orderStatusChangedMessageReceiver, int orderNo)
        {
            Request = request;
            OrderStatusChangedMessageReceiver = orderStatusChangedMessageReceiver;
            OrderNo = orderNo;

            DispatchedTime = DateTime.Now;
            LastAverageDealPrice = 0.0f;
            LastTotalDealVolume = 0;
            LastStatus = OrderStatus.NotSubmitted;

        }

        public DispatchedOrder Clone()
        {
            return new DispatchedOrder()
            {
                DispatchedTime = this.DispatchedTime,
                OrderNo = this.OrderNo,
                LastTotalDealVolume = this.LastTotalDealVolume,
                LastAverageDealPrice = this.LastAverageDealPrice,
                LastDeltaDealVolume = this.LastDeltaDealVolume,
                LastStatus = this.LastStatus,
                Request = this.Request,
                OrderStatusChangedMessageReceiver = this.OrderStatusChangedMessageReceiver
            };
        }
    }
}
