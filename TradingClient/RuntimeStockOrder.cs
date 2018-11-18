namespace TradingClient
{
    using StockAnalysis.StockTrading.Utility;

    sealed class RuntimeStockOrder
    {
        public string SecuritySymbol { get; private set; }

        public string SecurityName { get; private set; }

        public int ExpectedVolume { get; set; }

        public int RemainingVolume { get; set; }

        public BuyOrder AssociatedBuyOrder { get; set; }

        public SellOrder AssociatedSellOrder { get; set; }

        public StoplossOrder AssociatedStoplossOrder { get; set; }

        public RuntimeStockOrder(string symbol, string name)
        {
            SecuritySymbol = symbol;
            SecurityName = name;
        }
    }
}
