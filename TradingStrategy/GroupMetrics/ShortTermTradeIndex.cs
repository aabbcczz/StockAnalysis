using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition.Metrics;
using StockAnalysis.TradingStrategy;

namespace StockAnalysis.TradingStrategy.GroupMetrics
{
    /// <summary>
    /// TRIN (short term TRade INdex), or ARMS
    /// </summary>
    public sealed class ShortTermTradeIndex : GeneralGroupRuntimeMetricBase
    {
        private AdvanceDeclineLineAndRatio _adr;
        private AdvanceDeclineVolumeLineAndRatio _advr;

        public ShortTermTradeIndex(
            IEnumerable<ITradingObject> tradingObjects, 
            int windowSize, 
            string rawMetric = "ROC[1]")
            : base(tradingObjects)
        {
            if (windowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("window size must be greater than 0");
            }

            MetricNames = new string[] { "TRIN" };
            MetricValues = new double[] { 0.0 };
            DependedRawMetrics = new string[] { rawMetric , "BAR.VOL" };

            _adr = new AdvanceDeclineLineAndRatio(tradingObjects, windowSize, rawMetric);
            _advr = new AdvanceDeclineVolumeLineAndRatio(tradingObjects, windowSize, rawMetric);
        }

        public override void Update(IRuntimeMetric[][] metrics)
        {
            IRuntimeMetric[][] extractedMetrics = new IRuntimeMetric[1][];
            extractedMetrics[0] = metrics[0];

            _adr.Update(extractedMetrics);
            _advr.Update(metrics);

            MetricValues[0] = _adr.MetricValues[1] / _advr.MetricValues[1];
        }
    }
}
