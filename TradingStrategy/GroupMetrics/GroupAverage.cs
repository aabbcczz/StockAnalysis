namespace StockAnalysis.TradingStrategy.GroupMetrics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// average of group of raw metrics
    /// </summary>
    public sealed class GroupAverage : GeneralGroupRuntimeMetricBase
    {
        private Func<IRuntimeMetric, double> _selector;

        public GroupAverage(
            IEnumerable<ITradingObject> tradingObjects, 
            string rawMetric, 
            Func<IRuntimeMetric, double> selector = null)
            : base(tradingObjects)
        {
            MetricNames = new string[] { "GROUPAVERAGE" };
            MetricValues = new double[] { 0.0 };
            DependedRawMetrics = new string[] { rawMetric };

            _selector = selector ?? DefaultSelector;
        }

        private double DefaultSelector(IRuntimeMetric metric)
        {
            return metric.Values[0];
        }

        public override void Update(IRuntimeMetric[][] metrics)
        {
            CheckMetricsValidality(metrics);

            var rawMetrics = metrics[0];

            var groupSum = rawMetrics.Sum(m => m == null ? 0.0 : _selector(m));

            var groupCount = rawMetrics.Count(m => m != null);

            MetricValues[0] = groupCount == 0 ? 0.0 : groupSum / groupCount;
        }
    }
}
