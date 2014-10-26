namespace TradingStrategy.Strategy
{
    public interface IMarketEnteringComponent : ITradingStrategyComponent
    {
        bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
