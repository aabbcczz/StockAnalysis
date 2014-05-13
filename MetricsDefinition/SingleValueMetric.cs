using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    abstract class SingleValueMetric<InputType, OutputType> : Metric
    {
        public SingleValueMetric(params object[] parameters)
            : base(MetricType.SingleValueMetric, parameters)
        {
        }

        public abstract OutputType Calculate(IEnumerable<InputType> input);
    }
}
