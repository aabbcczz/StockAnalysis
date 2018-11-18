namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class TradingStrategyComponentResult
    {
        public string Comments { get; set; }

        /// <summary>
        /// if Price is null, it means using system default price.
        /// </summary>
        public TradingPrice Price { get; set; }

        public TradingStrategyComponentResult()
        {
            Comments = string.Empty;
            Price = null;
        }
    }
}
