namespace TradingClient.StrategyGDB
{
    public sealed class ExistingStock
    {
        public string SecuritySymbol { get; set; }
        public string SecurityName { get; set; }
        public int HoldDays { get; set; }
        public float StoplossPrice { get; set; }
        public int Volume { get; set; }
    }
}
