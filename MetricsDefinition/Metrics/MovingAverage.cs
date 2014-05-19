using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MA")]
    public sealed class MovingAverage : Metric
    {
        private int _lookback;

        public MovingAverage(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] result = new MovingSum(_lookback)
                .Calculate(input[0])
                .OperateThis((double)_lookback, (s, l) => s / l);

            return new double[1][] { result };
        }
    }
}
