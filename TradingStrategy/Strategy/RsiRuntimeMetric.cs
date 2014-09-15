using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
namespace TradingStrategy.Strategy
{
    public sealed class RsiRuntimeMetric : IRuntimeMetric
    {
        public double Rsi { get; private set; }

        private RelativeStrengthIndex _rsi;

        public RsiRuntimeMetric(int windowSize)
        {
            _rsi = new RelativeStrengthIndex(windowSize);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            Rsi = _rsi.Update(bar.ClosePrice);
        }
    }
}
