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
    /// 腾落指标（ADL/ADR). 与通常定义不同，此指标可作用于任意原始指标上，当原始指标为ROC[1]时即还原为腾落指标的初始定义
    /// 本指标用于统计原始指标大于0.0和小于0.0的个数差异和比例。
    /// </summary>
    public sealed class AdvanceDeclineLineAndRatio : GeneralGroupRuntimeMetricBase
    {
        private int _windowSize;
        private MovingSum _advancedSum;
        private MovingSum _declinedSum;

        public AdvanceDeclineLineAndRatio(
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

            MetricNames = new string[] { "ADL", "ADR" };
            MetricValues = new double[] { 0.0, 0.0 };
            DependedRawMetrics = new string[] { rawMetric };

            _advancedSum = new MovingSum(windowSize);
            _declinedSum = new MovingSum(windowSize);
        }

        public override void Update(IRuntimeMetric[][] metrics)
        {
            CheckMetricsValidality(metrics);

            var rawMetrics = metrics[0];

            var advanced = rawMetrics.Count(m => m != null && m.Values[0] > 0.0);
            var declined = rawMetrics.Count(m => m != null && m.Values[0] < 0.0);

            _advancedSum.Update(advanced);
            _declinedSum.Update(declined);

            // set ADL and ADR metric value
            var sumAdvanced = _advancedSum.Value;
            var sumDeclined = _declinedSum.Value;

            MetricValues[0] = sumAdvanced - sumDeclined;
            MetricValues[1] = Math.Abs(sumDeclined) < 1e-6 ? sumAdvanced : sumAdvanced / sumDeclined;
        }
    }
}
