using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("AD")]
    class AccumulationDistribution : IMetric
    {
        private int _lookback;
        
        static private int HighestPriceFieldIndex;
        static private int LowestPriceFieldIndex;
        static private int ClosePriceFieldIndex;
        static private int VolumeFieldIndex;


        static AccumulationDistribution()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            HighestPriceFieldIndex = attribute.NameToFieldIndexMap["HP"];
            LowestPriceFieldIndex = attribute.NameToFieldIndexMap["LP"];
            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
            VolumeFieldIndex = attribute.NameToFieldIndexMap["VOL"];
        }

        public AccumulationDistribution(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentException("lookback must be greater than 0");
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // AD can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("AD can only accept StockData's output as input");
            }

            double[] highestPrices = input[HighestPriceFieldIndex];
            double[] lowestPrices = input[LowestPriceFieldIndex];
            double[] closePrices = input[ClosePriceFieldIndex];
            double[] volumes = input[VolumeFieldIndex];
            double[] costs = new double[volumes.Length];

            for (int i = 0; i < costs.Length; ++i)
            {
                costs[i] = ((closePrices[i] - lowestPrices[i]) - (highestPrices[i] - closePrices[i])) / (highestPrices[i] - lowestPrices[i]) * volumes[i];
            }

            double sumOfVolume = 0.0;
            double sumOfCost = 0.0;

            double[] result = new double[volumes.Length];

            for (int i = 0; i < volumes.Length; ++i)
            {
                if (i < _lookback)
                {
                    sumOfVolume += volumes[i];
                    sumOfCost += costs[i];
                }
                else
                {
                    int j = i - _lookback + 1;

                    sumOfVolume += volumes[i] - volumes[j];
                    sumOfCost += costs[i] - costs[j];
                }

                result[i] = sumOfCost / sumOfVolume;
            }

            return new double[1][] { result };
        }
    }
}
