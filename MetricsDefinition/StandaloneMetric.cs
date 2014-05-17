using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace MetricsDefinition
{
    sealed class StandaloneMetric : MetricExpression
    {
        private IMetric _metric;

        public IMetric Metric { get { return _metric; } }

        public StandaloneMetric(IMetric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException("metric");
            }

            _metric = metric;
        }

        public override IEnumerable<double>[] Evaluate(IEnumerable<double>[] data)
        {
            return _metric.Calculate(data);
        }

        public override string[] GetFieldNames()
        {
            MetricAttribute attribute = _metric.GetType().GetCustomAttribute<MetricAttribute>();

            return attribute.NameToFieldIndexMap
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();
        }
    }
}
