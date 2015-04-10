namespace TradingStrategy.Base
{
    public interface IMarketEnteringComponent : ITradingStrategyComponent
    {
        bool CanEnter(ITradingObject tradingObject, out string comments, out object obj);
    }
}
