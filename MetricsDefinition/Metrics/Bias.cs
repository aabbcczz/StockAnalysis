using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("BIAS")]
    class Bias : IMetric
    {
        private int _lookback;

        public Bias(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
            double[] allData = input[0];

            double[] ma = new MovingAverage(_lookback).Calculate(input)[0];

            double[] result = new double[allData.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = (allData[i] - ma[i]) / ma[i];
            }

            return new double[1][] { result };
        }
    }
}
