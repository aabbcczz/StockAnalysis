using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("BOLL", "UB,MA,LB")]
    public sealed class BollingerBand : Metric
    {
        private int _lookback;
        private double _alpha;

        public BollingerBand(int lookback, double alpha)
        {
            if (lookback <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            if (alpha <= 0.0)
            {
                throw new ArgumentOutOfRangeException("alpha");
            }

            _lookback = lookback;
            _alpha = alpha;
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] ma = new MovingAverage(_lookback).Calculate(input[0]);
            double[] stddev = new StdDev(_lookback).Calculate(input[0]);

            double[] ub = MetricHelper.OperateNew(ma, stddev, (m, s) => m + _alpha * s);
            double[] lb = MetricHelper.OperateNew(ma, stddev, (m, s) => m - _alpha * s);

            return new double[3][] { ub, ma, lb };
        }
    }
}
