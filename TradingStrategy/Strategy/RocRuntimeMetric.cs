using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class RocRuntimeMetric : IRuntimeMetric
    {
        public double RateOfChange { get; private set; }

        private RateOfChange _roc;

        public RocRuntimeMetric(int windowSize)
        {
            _roc = new RateOfChange(windowSize);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            RateOfChange = _roc.Update(bar.ClosePrice);
        }
    }
}
