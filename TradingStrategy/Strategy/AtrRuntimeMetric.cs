using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class AtrRuntimeMetric : IRuntimeMetric
    {
        public double Atr { get; private set; }

        private readonly AverageTrueRange _atr;

        public AtrRuntimeMetric(int windowSize)
        {
            _atr = new AverageTrueRange(windowSize);
        }

        public void Update(Bar bar)
        {
            Atr = _atr.Update(bar);
        }
    }
}
