namespace StockAnalysis.TradingStrategy
{
    public interface IRuntimeMetricManagerObserver
    {
        void Observe(IRuntimeMetricManager manager);
    }
}
