namespace StockTradingConsole
{
    public sealed class OldStockForSerialization
    {
        public string SecuritySymbol { get; set; }
        public string SecurityName { get; set; }
        public int Volume { get; set; }

        public OldStockForSerialization()
        {
        }

        public OldStockForSerialization(OldStock os)
        {
            SecuritySymbol = os.Name.Symbol.NormalizedSymbol;
            SecurityName = os.Name.Names[0];
            Volume = os.Volume;
        }
    }
}
