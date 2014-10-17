namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedStopLossBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IStopLossComponent
        where T : IRuntimeMetric
    {
        public abstract double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments);
    }
}
