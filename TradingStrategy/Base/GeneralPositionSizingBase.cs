namespace StockAnalysis.TradingStrategy.Base
{
    public abstract class GeneralPositionSizingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionSizingComponent
    {
        public abstract PositionSizingComponentResult EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, int totalNumberOfObjectsToBeEstimated);

        public virtual int GetMaxPositionCount(int totalNumberOfObjectsToBeEstimated)
        {
            return totalNumberOfObjectsToBeEstimated;
        }
    }
}
