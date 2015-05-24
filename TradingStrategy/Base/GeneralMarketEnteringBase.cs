namespace TradingStrategy.Base
{
    public abstract class GeneralMarketEnteringBase 
        : GeneralTradingStrategyComponentBase
        , IMarketEnteringComponent
    {
        public abstract MarketEnteringComponentResult CanEnter(ITradingObject tradingObject);
    }
}
