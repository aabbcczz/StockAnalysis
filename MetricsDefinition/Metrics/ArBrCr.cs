using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("ABCR", "AR,BR,CR")]
    class ArBrCr : Metric
    {
        private int _lookback;

        public ArBrCr(int lookback)
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

            // ArBrCr can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("ArBrCr can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] op = input[StockData.OpenPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double sum = 0.0;

            double[] result = new double[cp.Length];


            return new double[1][] { result };
        }
    }
}
