namespace TradingStrategy.Strategy
{
    interface IMarketExitingComponent : ITradingStrategyComponent
    {
        bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
