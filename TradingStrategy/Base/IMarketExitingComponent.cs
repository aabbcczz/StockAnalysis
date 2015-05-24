namespace TradingStrategy.Base
{
    public sealed class MarketExitingComponentResult : TradingStrategyComponentResult
    {
        public bool ShouldExit { get; set; }

        public MarketExitingComponentResult()
            : base()
        {
            ShouldExit = false;
        }
    }

    public interface IMarketExitingComponent : ITradingStrategyComponent
    {
        MarketExitingComponentResult ShouldExit(ITradingObject tradingObject);
    }
}
