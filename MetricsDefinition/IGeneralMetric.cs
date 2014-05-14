using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    interface IGeneralMetric
    {
        IEnumerable<double> Calculate(IEnumerable<double> input);
    }
}
