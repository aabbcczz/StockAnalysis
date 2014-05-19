using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("OBV")]
    class OnBalanceVolume : Metric
    {
        public OnBalanceVolume()
        {
        }

        public override double[][] Calculate(double[][] input)
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

            double[] cp = input[StockData.ClosePriceFieldIndex];
            double[] volumes = input[StockData.VolumeFieldIndex];

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
                    obv += Math.Sign(cp[i] - cp[i - 1]) * volumes[i];
                }

                result[i] = obv;
            }

            return new double[1][] { result };
        }
    }
}
