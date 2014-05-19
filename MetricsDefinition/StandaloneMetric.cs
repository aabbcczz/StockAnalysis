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
        private Metric _metric;

        public Metric Metric { get { return _metric; } }

        public StandaloneMetric(Metric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException("metric");
            }

            _metric = metric;
        }

        public override double[][] Evaluate(double[][] data)
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
