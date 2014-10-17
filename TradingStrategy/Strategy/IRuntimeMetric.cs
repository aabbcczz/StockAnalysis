using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public interface IRuntimeMetric
    {
        void Update(Bar bar);
    }
}
