namespace TradingStrategy.Base
{
    public sealed class MarketEnteringComponentResult : TradingStrategyComponentResult
    {
        public bool CanEnter { get; set; }
        public object RelatedObject { get; set; }

        public MarketEnteringComponentResult()
            : base()
        {
            CanEnter = false;
            RelatedObject = null;
        }
    }

    public interface IMarketEnteringComponent : ITradingStrategyComponent
    {
        MarketEnteringComponentResult CanEnter(ITradingObject tradingObject);
    }
}
