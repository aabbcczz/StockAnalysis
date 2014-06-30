using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("CCI")]
    public sealed class CommodityChannelIndex : Metric
    {
        private const double Alpha = 0.015;

        private int _lookback;
        
        public CommodityChannelIndex(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // CCI can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("CCI can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double[] tp = MetricHelper.OperateNew(
                hp, lp, cp,
                (h, l, c) => { return (h + l + 2 * c) / 4; });

            double[] matp = new MovingAverage(_lookback).Calculate(tp);

            double[] d = new double[tp.Length];
            for (int i = 0; i < d.Length; ++i)
            {
                double sum = 0.0;
                for (int j = Math.Max(0, i - _lookback + 1); j <= i; ++j)
                {
                    sum += Math.Abs(tp[j] - matp[i]);
                }

                d[i] = sum / _lookback;
            }

            double[] result = tp.OperateThis(
                matp, d,
                (tpv, matpv, dv) => { return (tpv - matpv) / dv / Alpha; });


            return new double[1][] { result };
        }
    }
}
