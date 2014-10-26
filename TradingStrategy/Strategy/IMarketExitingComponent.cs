namespace TradingStrategy.Strategy
{
    public interface IMarketExitingComponent : ITradingStrategyComponent
    {
        bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
