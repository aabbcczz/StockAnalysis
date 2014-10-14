using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class LowBreakthroughRuntimeMetric : IRuntimeMetric
    {
        private Lowest _lowest;

        public double CurrentLowest { get; set; }

        public bool Breakthrough { get; set; }

        public LowBreakthroughRuntimeMetric(int windowSize)
        {
            _lowest = new Lowest(windowSize);
            CurrentLowest = 0.0;
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            double newLowest = _lowest.Update(bar.LowestPrice);

            Breakthrough = newLowest == bar.LowestPrice && newLowest < CurrentLowest;

            CurrentLowest = newLowest;
        }
    }
}
