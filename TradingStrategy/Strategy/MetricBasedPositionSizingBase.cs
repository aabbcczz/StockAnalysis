namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedPositionSizingBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IPositionSizingComponent
        where T : IRuntimeMetric
    {
        public abstract int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, out string comments);
    }
}
