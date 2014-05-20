using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("WVAD")]
    public sealed class WilliamVariableAccumulationDistribution : Metric
    {
        private int _lookback;
        
        public WilliamVariableAccumulationDistribution(int lookback)
        {
            // lookback 0 means infinity lookback
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

            // WVAD can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("WVAD can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] op = input[StockData.OpenPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];
            double[] volumes = input[StockData.VolumeFieldIndex];

            double[] indices = MetricHelper.OperateNew(hp, lp, cp, op, volumes,
                (h, l, c, o, v) => (c - o) * v / (h - l));

            double[] result = new MovingSum(_lookback).Calculate(indices);

            return new double[1][] { result };
        }
    }
}
