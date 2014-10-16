using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class RocRuntimeMetric : IRuntimeMetric
    {
        public double RateOfChange { get; private set; }

        private readonly RateOfChange _roc;

        public RocRuntimeMetric(int windowSize)
        {
            _roc = new RateOfChange(windowSize);
        }

        public void Update(Bar bar)
        {
            RateOfChange = _roc.Update(bar.ClosePrice);
        }
    }
}
