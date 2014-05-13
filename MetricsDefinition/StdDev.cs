using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    class StdDev : SingleValueMetric<double, double>
    {
        public StdDev()
            : base ()
        {
            // empty
        }

        public override double Calculate(IEnumerable<double> input)
        {
            if (input == null || input.Count() == 0)
            {
                throw new ArgumentNullException("input");
            }

            double average = input.Average();

            double sumOfSquare = 0.0;

            foreach(var data in input)
            {
                sumOfSquare += (data - average) * (data - average);
            }

            return Math.Sqrt(sumOfSquare / input.Count());
        }
    }
}
