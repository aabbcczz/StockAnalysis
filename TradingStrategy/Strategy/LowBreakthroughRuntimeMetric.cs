using System;
using MetricsDefinition.Metrics;

namespace TradingStrategy.Strategy
{
    public sealed class LowBreakthroughRuntimeMetric : IRuntimeMetric
    {
        private readonly Lowest _lowest;

        public double CurrentLowest { get; private set; }

        public bool Breakthrough { get; private set; }

        public LowBreakthroughRuntimeMetric(int windowSize)
        {
            _lowest = new Lowest(windowSize);
            CurrentLowest = 0.0;
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            double newLowest = _lowest.Update(bar.LowestPrice);

            Breakthrough = Math.Abs(newLowest - bar.LowestPrice) < 1e-6;

            CurrentLowest = newLowest;
        }
    }
}
