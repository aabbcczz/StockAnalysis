using StockAnalysis.Common.Data;

namespace StockAnalysis.TradingStrategy
{
    public interface IRuntimeMetric
    {
        double[] Values { get; }

        void Update(Bar bar);
    }
}
