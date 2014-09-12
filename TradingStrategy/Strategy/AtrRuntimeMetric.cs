using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
namespace TradingStrategy.Strategy
{
    public sealed class AtrRuntimeMetric : IRuntimeMetric
    {
        public double Atr { get; private set; }

        private AverageTrueRange _atr;

        public AtrRuntimeMetric(int windowSize)
        {
            _atr = new AverageTrueRange(windowSize);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            Atr = _atr.Update(bar);
        }
    }
}
