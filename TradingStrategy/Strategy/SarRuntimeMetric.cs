﻿using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class SarRuntimeMetric : IRuntimeMetric
    {
        private readonly StopAndReverse _sar;

        public double Sar { get; private set; }

        public SarRuntimeMetric(
            int windowSize = 4, 
            double accelateFactor = 0.02, 
            double accelateFactorStep = 0.02,
            double maxAcceleteFactor = 0.2)
        {
            _sar = new StopAndReverse(windowSize, accelateFactor, accelateFactorStep, maxAcceleteFactor);
        }

        public void Update(Bar bar)
        {
            Sar = _sar.Update(bar);
        }
    }
}
