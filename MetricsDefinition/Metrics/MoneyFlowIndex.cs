using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition.Metrics
{
    [Metric("MFI")]
    class MoneyFlowIndex : IMetric
    {
        private int _lookback;
        
        static private int HighestPriceFieldIndex;
        static private int LowestPriceFieldIndex;
        static private int ClosePriceFieldIndex;
        static private int VolumeFieldIndex;


        static MoneyFlowIndex()
        {
            MetricAttribute attribute = typeof(StockData).GetCustomAttribute<MetricAttribute>();

            HighestPriceFieldIndex = attribute.NameToFieldIndexMap["HP"];
            LowestPriceFieldIndex = attribute.NameToFieldIndexMap["LP"];
            ClosePriceFieldIndex = attribute.NameToFieldIndexMap["CP"];
            VolumeFieldIndex = attribute.NameToFieldIndexMap["VOL"];
        }

        public MoneyFlowIndex(int lookback)
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

            // MFI can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("MoneyFlowIndex can only accept StockData's output as input");
            }

            double[] highestPrices = input[HighestPriceFieldIndex];
            double[] lowestPrices = input[LowestPriceFieldIndex];
            double[] closePrices = input[ClosePriceFieldIndex];
            double[] volumes = input[VolumeFieldIndex];
            double[] truePrices = new double[volumes.Length];
            for (int i = 0; i < truePrices.Length; ++i)
            {
                truePrices[i] = (highestPrices[i] + lowestPrices[i] + closePrices[i]) / 3;
            }

            double[] positiveMoneyFlow = new double[truePrices.Length];
            double[] negativeMoneyFlow = new double[truePrices.Length];

            positiveMoneyFlow[0] = truePrices[0] * volumes[0];
            negativeMoneyFlow[0] = 1e-6; // set a very small number to avoid dividing by zero

            for (int i = 1; i < truePrices.Length; ++i)
            {
                if (truePrices[i] > truePrices[i - 1])
                {
                    positiveMoneyFlow[i] = truePrices[i] * volumes[i];
                    negativeMoneyFlow[i] = 0.0;
                }
                else if (truePrices[i] < truePrices[i - 1])
                {
                    positiveMoneyFlow[i] = 0.0;
                    negativeMoneyFlow[i] = truePrices[i] * volumes[i];
                }
                else
                {
                    positiveMoneyFlow[i] = negativeMoneyFlow[i] = 1e-6;
                }
            }

            double sumOfPmf = 0.0;
            double sumOfNmf = 0.0;

            double[] result = new double[volumes.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                if (i < _lookback)
                {
                    sumOfPmf += positiveMoneyFlow[i];
                    sumOfNmf += negativeMoneyFlow[i];
                }
                else
                {
                    int j = i - _lookback + 1;

                    sumOfPmf += positiveMoneyFlow[i] - positiveMoneyFlow[j];
                    sumOfNmf += negativeMoneyFlow[i] - negativeMoneyFlow[j];
                }

                result[i] = 100.0 / (1.0 + sumOfPmf / sumOfNmf);
            }

            return new double[1][] { result };
        }
    }
}
