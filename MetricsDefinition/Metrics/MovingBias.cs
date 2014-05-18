using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MB")]
    class MovingBias : IMetric
    {
        private int _shortLookback;
        private int _longLookback;

        public MovingBias(int shortLookback, int longLookback)
        {
            if (shortLookback <= 0 || longLookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            if (shortLookback >= longLookback)
            {
                throw new ArgumentException("short lookback should be smaller than long lookback");
            }

            _shortLookback = shortLookback;
            _longLookback = longLookback;
        }

        public double[][] Calculate(double[][] input)
        {
            double[] allData = input[0];

            double[] maShort = new MovingAverage(_shortLookback).Calculate(input)[0];
            double[] maLong = new MovingAverage(_longLookback).Calculate(input)[0];

            double[] result = new double[allData.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = maShort[i] - maLong[i];
            }

            return new double[1][] { result };
        }
    }
}
