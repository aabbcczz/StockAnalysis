namespace TradingStrategy.Strategy
{
    public abstract class GeneralMarketEnteringBase 
        : GeneralTradingStrategyComponentBase
        , IMarketEnteringComponent
    {
        public abstract bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
