using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("S", "OP,CP,HP,LP,VOL,AMT")]
    public sealed class StockData : IMetric
    {
        public const int FieldCount = 6;

        public IEnumerable<double>[] Calculate(IEnumerable<double>[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length < FieldCount)
            {
                throw new ArgumentException("input has less number of fields than expected");
            }

            return input.Take(FieldCount).ToArray();
        }
    }
}
