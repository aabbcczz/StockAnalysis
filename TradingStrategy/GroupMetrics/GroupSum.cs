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

            _selector = selector;
        }

        public override void Update(IRuntimeMetric[][] metrics)
        {
            CheckMetricsValidality(metrics);

            var rawMetrics = metrics[0];

            var groupSum = _selector == null
                ? rawMetrics.Sum(m => m == null ? 0.0 : m.Values[0])
                : rawMetrics.Sum(m => _selector(m));


            MetricValues[0] = groupSum;
        }
    }
}
