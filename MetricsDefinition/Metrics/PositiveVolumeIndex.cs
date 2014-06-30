using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("PVI")]
    public sealed class PositiveVolumeIndex : Metric
    {
        public PositiveVolumeIndex()
        {
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // PVI can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("PositiveVolumeIndex can only accept StockData's output as input");
            }

            double[] cp = input[StockData.ClosePriceFieldIndex];
            double[] volumes = input[StockData.VolumeFieldIndex];

            double[] result = new double[volumes.Length];

            result[0] = 100.0;
            for (int i = 1; i < result.Length; ++i)
            {
                if (volumes[i] > volumes[i - 1])
                {
                    result[i] = result[i - 1] * cp[i] / cp[i - 1];
                }
                else
                {
                    result[i] = result[i - 1];
                }
            }

            return new double[1][] { result };
        }
    }
}
