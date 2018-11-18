namespace StockAnalysis.TradingStrategy.GroupMetrics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// sum of group of raw metrics
    /// </summary>
    public sealed class GroupSum : GeneralGroupRuntimeMetricBase
    {
        private Func<IRuntimeMetric, double> _selector;

        public GroupSum(
            IEnumerable<ITradingObject> tradingObjects, 
            string rawMetric, 
            Func<IRuntimeMetric, double> selector = null)
            : base(tradingObjects)
        {
            MetricNames = new string[] { "GROUPSUM" };
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

            MetricValues[0] = groupSum;
        }
    }
}
