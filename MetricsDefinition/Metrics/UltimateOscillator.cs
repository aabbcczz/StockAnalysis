using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("UOS")]
    public sealed class UltimateOscillator : Metric
    {
        private int[] _lookback = new int[3];
        private double[] _weight = new double[3];

        public UltimateOscillator(int lookback1, int lookback2, int lookback3, double weight1, double weight2, double weight3)
        {
            if (lookback1 <= 0 || lookback2 <= 0 || lookback3 <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            if (weight1 < 0.0 || weight2 < 0.0 || weight3 < 0.0)
            {
                throw new ArgumentOutOfRangeException("weight");
            }

            _lookback[0] = lookback1;
            _lookback[1] = lookback2;
            _lookback[2] = lookback3;
            _weight[0] = weight1;
            _weight[1] = weight2;
            _weight[2] = weight3;
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // UOS can only accept StockData's output as input
            if (input.Length != StockData.FieldCount)
            {
                throw new ArgumentException("UOS can only accept StockData's output as input");
            }

            double[] hp = input[StockData.HighestPriceFieldIndex];
            double[] lp = input[StockData.LowestPriceFieldIndex];
            double[] cp = input[StockData.ClosePriceFieldIndex];

            double[] tr = new double[hp.Length];
            double[] bp = new double[hp.Length];

            double prevCp = 0.0;
            for (int i = 0; i < hp.Length; ++i)
            {
                bp[i] = cp[i] - Math.Min(lp[i], prevCp);

                tr[i] = Math.Max(hp[i], prevCp) - Math.Min(lp[i], prevCp);

                prevCp = cp[i];
            }

            double[][] average = new double[3][];
            for (int i = 0; i < 3; ++i)
            {
                average[i] = MetricHelper.OperateNew(
                    new MovingSum(_lookback[i]).Calculate(bp),
                    new MovingSum(_lookback[i]).Calculate(tr),
                    (b, t) => b / t);
            }

            double sumWeight = (_weight[0] + _weight[1] + _weight[2]) / 100.0;

            double[] result = average[0].OperateThis(
                average[1],
                average[2],
                (a1, a2, a3) => (_weight[0] * a1 + _weight[1] * a2 + _weight[2] * a3) / sumWeight);

            return new double[1][] { result };
        }
    }
}
