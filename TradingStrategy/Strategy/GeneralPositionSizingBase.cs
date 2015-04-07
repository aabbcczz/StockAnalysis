namespace TradingStrategy.Strategy
{
    public abstract class GeneralPositionSizingBase 
        : GeneralTradingStrategyComponentBase
        , IPositionSizingComponent
    {
        public abstract int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, out string comments, int totalNumberOfObjectsToBeEstimated);
    }
}
