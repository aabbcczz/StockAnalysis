namespace TradingStrategy.Strategy
{
    interface IMarketEnteringComponent : ITradingStrategyComponent
    {
        bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
