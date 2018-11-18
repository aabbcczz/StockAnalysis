namespace StockAnalysis.TradingStrategy
{
    public interface IGroupRuntimeMetricManagerObserver
    {
        void Observe(IGroupRuntimeMetricManager manager);
    }
}
