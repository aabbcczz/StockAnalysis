using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("SD")]
    public sealed class StdDev : IMetric
    {
        private int _lookback;

        public StdDev(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback must be greater than zero");
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input[0];
            double[] output = new double[allData.Length];
            double sum = 0.0;

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _lookback)
                {
                    sum += allData[i];

                    output[i] = CalculateStdDev(sum, allData, 0, i);
                }
                else
                {
                    sum = sum - allData[i - _lookback] + allData[i];

                    output[i] = CalculateStdDev(sum, allData, i - _lookback + 1, i);
                }
            }

            return new double[1][] { output };
        }

        private double CalculateStdDev(double sum, double[] data, int startIndex, int endIndex)
        {
            int count = endIndex - startIndex + 1;

            double average = sum / count;
            double sumOfSquares = 0.0;

            for (int j = startIndex; j <= endIndex; ++j)
            {
                sumOfSquares += (data[j] - average) * (data[j] - average);
            }

            return Math.Sqrt(sumOfSquares / count);
        }
    }
}
