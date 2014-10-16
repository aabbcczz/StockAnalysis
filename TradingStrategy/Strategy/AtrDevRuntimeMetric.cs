using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class AtrDevRuntimeMetric : IRuntimeMetric
    {
        public double Atr { get; private set; }
        public double Sdtr { get; private set; }

        private readonly AverageTrueRange _atr;
        private readonly StdDevTrueRange _sdtr;

        public AtrDevRuntimeMetric(int windowSize)
        {
            _atr = new AverageTrueRange(windowSize);
            _sdtr = new StdDevTrueRange(windowSize);
        }

        public void Update(Bar bar)
        {
            Atr = _atr.Update(bar);
            Sdtr = _sdtr.Update(bar);
        }
    }
}
