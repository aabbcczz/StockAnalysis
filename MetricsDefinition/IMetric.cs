using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    interface IMetric
    {
        double[][] Calculate(double[][] input);
    }
}
