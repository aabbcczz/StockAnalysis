using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class SarRuntimeMetric : IRuntimeMetric
    {
        private StopAndReverse _sar;

        public double Sar { get; private set; }

        public SarRuntimeMetric(
            int windowSize = 4, 
            double accelateFactor = 0.02, 
            double accelateFactorStep = 0.02,
            double maxAcceleteFactor = 0.2)
        {
            _sar = new StopAndReverse(windowSize, accelateFactor, accelateFactorStep, maxAcceleteFactor);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            Sar = _sar.Update(bar);
        }
    }
}
