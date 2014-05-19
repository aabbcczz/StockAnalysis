using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("RSI")]
    public sealed class RelativeStrengthIndex : Metric
    {
        private int _lookback;

        public RelativeStrengthIndex(int lookback)
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

            double[] cp = input[0];
            double[] uc = new double[cp.Length];
            double[] dc = new double[cp.Length];

            uc[0] = dc[0] = 0.0;
            for (int i = 1; i < cp.Length; ++i)
            {
                double diff = cp[i] - cp[i - 1];
                uc[i] = Math.Max(0.0, diff);
                dc[i] = Math.Max(0.0, -diff);
            }

            double[] msuc = new MovingSum(_lookback).Calculate(uc);
            double[] msdc = new MovingSum(_lookback).Calculate(dc);

            double[] result = msuc.OperateThis(
                msdc, 
                (u, d) =>
                {
                    double sum = u + d;
                    return sum == 0.0 ? 0.0 : u / sum * 100.0;
                });

            return new double[1][] { result };
        }
    }
}
