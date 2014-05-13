using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public abstract class SequentialValueMetric<InputType, OutputType> : Metric
    {
        public SequentialValueMetric(params object[] parameters)
            : base(MetricType.SequentialValueMetric, parameters)
        {
        }

        public abstract IEnumerable<OutputType> Calculate(IEnumerable<InputType> input);
    }
}
