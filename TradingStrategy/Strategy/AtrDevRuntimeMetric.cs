using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
namespace TradingStrategy.Strategy
{
    public sealed class AtrDevRuntimeMetric : IRuntimeMetric
    {
        public double Atr { get; private set; }
        public double Sdtr { get; private set; }

        private AverageTrueRange _atr;
        private StdDevTrueRange _sdtr;

        public AtrDevRuntimeMetric(int windowSize)
        {
            _atr = new AverageTrueRange(windowSize);
            _sdtr = new StdDevTrueRange(windowSize);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            Atr = _atr.Update(bar);
            Sdtr = _sdtr.Update(bar);
        }
    }
}
