namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class GeneralMarketExitingBase 
        : GeneralTradingStrategyComponentBase
        , IMarketExitingComponent
    {
        public abstract MarketExitingComponentResult ShouldExit(ITradingObject tradingObject);
    }
}
