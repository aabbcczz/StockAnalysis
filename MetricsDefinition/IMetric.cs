using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public abstract class Metric
    {
        public abstract double[][] Calculate(double[][] input);

        public double[] Calculate(double[] input)
        {
            return Calculate(new double[1][] { input })[0];
        }
    }
}
