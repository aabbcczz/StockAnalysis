namespace StockAnalysis.TradingStrategy
{
    using Common.Data;

    public interface IRuntimeMetric
    {
        double[] Values { get; }

        void Update(Bar bar);
    }
}
