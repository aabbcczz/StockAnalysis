namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedMarketExitingBase<T>
        : MetricBasedTradingStrategyComponentBase<T>
        , IMarketExitingComponent
        where T : IRuntimeMetric
    {
        public abstract bool ShouldExit(ITradingObject tradingObject, out string comments);
    }
}
