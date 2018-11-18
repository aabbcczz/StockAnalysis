namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class GeneralBuyPriceFilteringBase : 
        GeneralTradingStrategyComponentBase, 
        IBuyPriceFilteringComponent
    {
        public abstract BuyPriceFilteringComponentResult IsPriceAcceptable(ITradingObject tradingObject, double price);
    }
}
