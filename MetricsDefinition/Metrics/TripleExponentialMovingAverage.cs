using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("TRIX,TEMA")]
    class TripleExponentialMovingAverage : IMetric
    {
        private int _lookback;

        public TripleExponentialMovingAverage(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
            ExponentialMovingAverage ema = new ExponentialMovingAverage(_lookback);

            return ema.Calculate(ema.Calculate(ema.Calculate(input)));
        }
    }
}
