using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("WVAD")]
    class WilliamVariableAccumulationDistribution : IMetric
    {
        private int _lookback;
        
        static private int HighestPriceFieldIndex;
        static private int LowestPriceFieldIndex;
        static private int OpenPriceFieldIndex;
        static private int ClosePriceFieldIndex;
        static private int VolumeFieldIndex;


        static WilliamVariableAccumulationDistribution()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            HighestPriceFieldIndex = attribute.NameToFieldIndexMap["HP"];
            LowestPriceFieldIndex = attribute.NameToFieldIndexMap["LP"];
            OpenPriceFieldIndex = attribute.NameToFieldIndexMap["OP"];
            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
            VolumeFieldIndex = attribute.NameToFieldIndexMap["VOL"];
        }

        public WilliamVariableAccumulationDistribution(int lookback)
        {
            // lookback 0 means infinity lookback
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

            // WVAD can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("WVAD can only accept StockData's output as input");
            }

            double[] highestPrices = input[HighestPriceFieldIndex];
            double[] lowestPrices = input[LowestPriceFieldIndex];
            double[] openPrices = input[OpenPriceFieldIndex];
            double[] closePrices = input[ClosePriceFieldIndex];
            double[] volumes = input[VolumeFieldIndex];
            double[] indices = new double[volumes.Length];
            for (int i = 0; i < indices.Length; ++i)
            {
                indices[i] = (closePrices[i] - openPrices[i]) * volumes[i] / (highestPrices[i] - lowestPrices[i]);
            }

            double sum = 0.0;

            double[] result = new double[volumes.Length];

            for (int i = 0; i < volumes.Length; ++i)
            {
                if (i < _lookback)
                {
                    sum += indices[i];
                }
                else
                {
                    int j = i - _lookback + 1;

                    sum += indices[i] - indices[j];
                }

                result[i] = sum;
            }

            return new double[1][] { result };
        }
    }
}
