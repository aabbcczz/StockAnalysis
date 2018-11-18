namespace StockTradingConsole
{
    using StockAnalysis.Common.SymbolName;

    public sealed class OldStock
    {
        public StockName Name { get; set; }
        public int Volume { get; set; }

        public OldStock()
        {
        }

        public OldStock(OldStockForSerialization oss)
        {
            Name = new StockName(oss.SecuritySymbol, oss.SecurityName);
            Volume = Volume;
        }
    }
}
