using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using System.Reflection;

namespace MetricsDefinition
{
    /// <summary>
    /// The Chavkin metric
    /// </summary>
    [Metric("CV")]
    public sealed class Chavkin : Metric
    {
        private int _lookback;
        private int _interval;

        public Chavkin(int lookback, int interval)
        {
            if (lookback <= 1 || interval <= 0 || interval > lookback)
            {
                throw new ArgumentOutOfRangeException();
            }

            _lookback = lookback;
            _interval = interval;
        }

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // Chavkin can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("Chavkin can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double[] mahl = new ExponentialMovingAverage(_lookback)
                .Calculate(MetricHelper.OperateNew(hp, lp, (h, l) => h - l));

            double[] result = new double[hp.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                if (i < _interval)
                {
                    result[i] = (mahl[i] - mahl[0]) / mahl[0] * 100.0;
                }
                else
                {
                    result[i] = (mahl[i] - mahl[i - _interval + 1]) / mahl[i - _interval + 1] * 100.0;
                }
            }

            return new double[1][] { result };
        }
    }
}
