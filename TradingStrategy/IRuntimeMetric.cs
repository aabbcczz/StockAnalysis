using StockAnalysis.Common.Data;

namespace TradingStrategy
{
    public interface IRuntimeMetric
    {
        double[] Values { get; }

        void Update(Bar bar);
    }
}
