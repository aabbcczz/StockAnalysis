using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("AD")]
    public sealed class AccumulationDistribution : Metric
    {
        private int _lookback;

        public AccumulationDistribution(int lookback)
        {
            if (lookback <= 0)
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

            // AD can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("AD can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];
            double[] volumes = input[StockData.VolumeFieldIndex];
            double[] costs = MetricHelper.OperateNew(
                hp,
                cp,
                lp,
                volumes,
                (h, c, l, vol) => { return ((c - l) - (h - c)) / (h - l) * vol; });

            var result = MetricHelper.OperateNew(
                new MovingSum(_lookback).Calculate(volumes),
                new MovingSum(_lookback).Calculate(costs),
                (v, c) => c / v);

            return new double[1][] { result };
        }
    }
}
