namespace StockAnalysis.StockTrading.Utility
{
    public sealed class OrderStatusChangedMessage
    {
        public DispatchedOrder Order { get; set; }

        public OrderStatus PreviousStatus { get; set; }

        public OrderStatus CurrentStatus { get; set; }
    }
}
