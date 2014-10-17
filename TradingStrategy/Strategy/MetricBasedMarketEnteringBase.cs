namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedMarketEnteringBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IMarketEnteringComponent
        where T : IRuntimeMetric
    {
        public abstract bool CanEnter(ITradingObject tradingObject, out string comments);
    }
}
