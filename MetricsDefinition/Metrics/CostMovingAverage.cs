using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace MetricsDefinition
{

    [Metric("COSTMA,CYC,CMA")]
    class CostMovingAverage : IMetric
    {
        // lookback 0 means infinity lookback
        private int _lookback;
        
        // because we are processing data with right recovered, the amount can't be used
        // and we need to estimate the price by averging HP/LP/OP/CP.
        static private int HighestPriceFieldIndex;
        static private int LowestPriceFieldIndex;
        static private int OpenPriceFieldIndex;
        static private int ClosePriceFieldIndex;
        static private int VolumeFieldIndex;


        static CostMovingAverage()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            HighestPriceFieldIndex = attribute.NameToFieldIndexMap["HP"];
            LowestPriceFieldIndex = attribute.NameToFieldIndexMap["LP"];
            OpenPriceFieldIndex = attribute.NameToFieldIndexMap["OP"];
            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
            VolumeFieldIndex = attribute.NameToFieldIndexMap["VOL"];
        }

        public CostMovingAverage(int lookback)
        {
            // lookback 0 means infinity lookback
            if (lookback < 0)
            {
                throw new ArgumentException("days must be greater than 0");
            }

            _lookback = lookback;
        }

        public IEnumerable<double>[] Calculate(IEnumerable<double>[] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // ATR can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("COSTMA can only accept StockData's output as input");
            }

            double[] highestPrices = input[HighestPriceFieldIndex].ToArray();
            double[] lowestPrices = input[LowestPriceFieldIndex].ToArray();
            double[] openPrices = input[OpenPriceFieldIndex].ToArray();
            double[] closePrices = input[ClosePriceFieldIndex].ToArray();
            double[] volumes = input[VolumeFieldIndex].ToArray();
            double[] averagePrices = Enumerable
                .Range(0, volumes.Length)
                .Select(i => (highestPrices[i] + lowestPrices[i] + openPrices[i] + closePrices[i]) / 4)
                .ToArray();

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
                        int j = i - _lookback + 1;

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
