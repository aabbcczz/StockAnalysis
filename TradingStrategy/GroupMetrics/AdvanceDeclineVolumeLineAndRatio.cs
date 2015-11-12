using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition.Metrics;
using TradingStrategy;

namespace TradingStrategy.GroupMetrics
{
    /// <summary>
    /// 成交量腾落指标（ADVL/ADVR). 与通常定义不同，此指标可作用于任意原始指标上，当原始指标为ROC[1]时即还原为腾落指标的初始定义
    /// 本指标用于统计原始指标大于0.0和小于0.0的个数差异和比例。
    /// </summary>
    public sealed class AdvanceDeclineVolumeLineAndRatio : GeneralGroupRuntimeMetricBase
    {
        private int _windowSize;
        private MovingSum _advancedSum;
        private MovingSum _declinedSum;

        public AdvanceDeclineVolumeLineAndRatio(
            IEnumerable<ITradingObject> tradingObjects, 
            int windowSize, 
            string rawMetric = "ROC[1]")
            : base(tradingObjects)
        {
            if (windowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("window size must be greater than 0");
            }

            _windowSize = windowSize;

            MetricNames = new string[] { "ADVL", "ADVR" };
            MetricValues = new double[] { 0.0, 0.0 };
            DependedRawMetrics = new string[] { rawMetric, "BAR.VOL" };

            _advancedSum = new MovingSum(windowSize);
            _declinedSum = new MovingSum(windowSize);
        }

        public override void Update(IRuntimeMetric[][] metrics)
        {
            CheckMetricsValidality(metrics);

            var rawMetrics = metrics[0];
            var volumes = metrics[1];

            var advancedVolume = Enumerable.Range(0, rawMetrics.Length)
                .Sum(i => rawMetrics[i] != null && rawMetrics[i].Values[0] > 0.0 ? volumes[i].Values[0] : 0.0);

            var declinedVolume = Enumerable.Range(0, rawMetrics.Length)
                .Sum(i => rawMetrics[i] != null && rawMetrics[i].Values[0] < 0.0 ? volumes[i].Values[0] : 0.0);

            _advancedSum.Update(advancedVolume);
            _declinedSum.Update(declinedVolume);

            // set ADVL and ADVR metric value
            var sumAdvancedVolume = _advancedSum.Value;
            var sumDeclinedVolume = _declinedSum.Value;

            MetricValues[0] = sumAdvancedVolume - sumDeclinedVolume;
            MetricValues[1] = Math.Abs(sumDeclinedVolume) < 1e-6 ? sumAdvancedVolume : sumAdvancedVolume / sumDeclinedVolume;
        }
    }
}
