namespace TradingStrategy.Base
{
    public abstract class GeneralMarketExitingBase 
        : GeneralTradingStrategyComponentBase
        , IMarketExitingComponent
    {
        public abstract bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
