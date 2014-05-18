using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("S", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class StockData : IMetric
    {
        public const int FieldCount = 6;

        public double[][] Calculate(double[][] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length != FieldCount)
            {
                throw new ArgumentException("input has different number of fields than expected");
            }

            return input;
        }
    }
}
