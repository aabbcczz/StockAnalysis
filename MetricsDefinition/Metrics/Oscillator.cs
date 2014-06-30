using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("OSC")]
    public sealed class Oscillator : Metric
    {
        private int _lookback;

        public Oscillator(int lookback)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback = lookback;
        }

        public override double[][] Calculate(double[][] input)
        {
            double[] ma = new MovingAverage(_lookback).Calculate(input[0]);

            double[] result = ma.OperateThis(input[0], (m, i) => { return i - m; });

            return new double[1][] { result };
        }
    }
}
