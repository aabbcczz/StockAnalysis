using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public enum MetricType
    {
        Unknown = 0,
        SingleValueMetric, // metric that is a single value for a sequence of input
        SequentialValueMetric // metric that is a sequence of values for a sequence of input
    }
}
