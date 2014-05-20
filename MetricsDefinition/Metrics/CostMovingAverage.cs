using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace MetricsDefinition
{

    [Metric("COSTMA,CYC,CMA")]
    public sealed class CostMovingAverage : Metric
    {
        // lookback 0 means infinity lookback
        private int _lookback;
        
        public CostMovingAverage(int lookback)
        {
            // lookback 0 means infinity lookback
            if (lookback < 0)
            {
                throw new ArgumentException("lookback must be greater than 0");
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // CMA can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("COSTMA can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] op = input[StockData.OpenPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];
            double[] volumes = input[StockData.VolumeFieldIndex];

            double[] averagePrices = MetricHelper.OperateNew(
                hp, lp, cp,
                (h, l, c) => { return (h + l + 2 * c) / 4; });


            double sumOfVolume = 0.0;
            double sumOfCost = 0.0;

            double[] result = new double[volumes.Length];

            for (int i = 0; i < volumes.Length; ++i)
            {
                if (_lookback == 0)
                {
                    sumOfVolume += volumes[i];
                    sumOfCost += volumes[i] * averagePrices[i];
                }
                else
                {
                    if (i < _lookback)
                    {
                        sumOfVolume += volumes[i];
                        sumOfCost += volumes[i] * averagePrices[i];
                    }
                    else
                    {
                        int j = i - _lookback;

                        sumOfVolume += volumes[i] - volumes[j];
                        sumOfCost += volumes[i] * averagePrices[i] - volumes[j] * averagePrices[j];
                    }
                }

                result[i] = sumOfCost / sumOfVolume;
            }

            return new double[1][] { result };
        }
    }
}
