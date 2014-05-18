using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("NVI")]
    class NegativeVolumeIndex : IMetric
    {
        static private int ClosePriceFieldIndex;
        static private int VolumeFieldIndex;


        static NegativeVolumeIndex()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
            VolumeFieldIndex = attribute.NameToFieldIndexMap["VOL"];
        }

        public NegativeVolumeIndex()
        {
        }

        public double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // NVI can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("NegativeVolumeIndex can only accept StockData's output as input");
            }

            double[] closePrices = input[ClosePriceFieldIndex];
            double[] volumes = input[VolumeFieldIndex];

            double[] result = new double[volumes.Length];

            result[0] = 100.0;
            for (int i = 1; i < result.Length; ++i)
            {
                if (volumes[i] < volumes[i - 1])
                {
                    result[i] = result[i - 1] * closePrices[i] / closePrices[i - 1];
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
