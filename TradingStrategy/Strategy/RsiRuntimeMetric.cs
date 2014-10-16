using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class RsiRuntimeMetric : IRuntimeMetric
    {
        public double Rsi { get; private set; }

        private readonly RelativeStrengthIndex _rsi;

        public RsiRuntimeMetric(int windowSize)
        {
            _rsi = new RelativeStrengthIndex(windowSize);
        }

        public void Update(Bar bar)
        {
            Rsi = _rsi.Update(bar.ClosePrice);
        }
    }
}
