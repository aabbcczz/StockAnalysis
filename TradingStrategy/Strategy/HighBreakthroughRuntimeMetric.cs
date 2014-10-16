using System;
using MetricsDefinition.Metrics;

namespace TradingStrategy.Strategy
{
    public sealed class HighBreakthroughRuntimeMetric : IRuntimeMetric
    {
        private readonly Highest _highest;

        public double CurrentHighest { get; private set; }

        public bool Breakthrough { get; private set; }

        public HighBreakthroughRuntimeMetric(int windowSize)
        {
            _highest = new Highest(windowSize);
            CurrentHighest = 0.0;
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            double newHighest = _highest.Update(bar.HighestPrice);

            Breakthrough = Math.Abs(newHighest - bar.HighestPrice) < 1e-6 && newHighest > CurrentHighest;

            CurrentHighest = newHighest;
        }
    }
}
