using StockAnalysis.Share;

namespace TradingStrategy
{
    public interface IRuntimeMetric
    {
        double[] Values { get; }

        void Update(Bar bar);
    }
}
