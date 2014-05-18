using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("OBV")]
    class OnBalanceVolume : IMetric
    {
        // because we are processing data with right recovered, the amount can't be used
        // and we need to estimate the price by averging HP/LP/OP/CP.
        static private int ClosePriceFieldIndex;
        static private int VolumeFieldIndex;


        static OnBalanceVolume()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
            VolumeFieldIndex = attribute.NameToFieldIndexMap["VOL"];
        }

        public OnBalanceVolume()
        {
        }

        public double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // OBV can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("OBV can only accept StockData's output as input");
            }

            double[] closePrices = input[ClosePriceFieldIndex];
            double[] volumes = input[VolumeFieldIndex];

            double obv = 0.0;
            double[] result = new double[volumes.Length];

            for (int i = 0; i < volumes.Length; ++i)
            {
                if (i == 0)
                {
                    obv = volumes[i];
                }
                else
                {
                    obv += Math.Sign(closePrices[i] - closePrices[i - 1]) * volumes[i];
                }

                result[i] = obv;
            }

            return new double[1][] { result };
        }
    }
}
