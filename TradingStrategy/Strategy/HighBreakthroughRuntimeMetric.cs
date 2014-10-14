using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class HighBreakthroughRuntimeMetric : IRuntimeMetric
    {
        private Highest _highest;

        public double CurrentHighest { get; set; }

        public bool Breakthrough { get; set; }

        public HighBreakthroughRuntimeMetric(int windowSize)
        {
            _highest = new Highest(windowSize);
            CurrentHighest = 0.0;
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            double newHighest = _highest.Update(bar.HighestPrice);

            Breakthrough = newHighest == bar.HighestPrice && newHighest > CurrentHighest;

            CurrentHighest = newHighest;
        }
    }
}
