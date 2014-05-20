using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("KST")]
    public sealed class KnowSureThings : Metric
    {
        private int _lookback1;
        private int _lookback2;
        private int _lookback3;
        private int _lookback4;

        public KnowSureThings(int lookback1, int lookback2, int lookback3, int lookback4)
        {
            if (lookback1 <= 0 || lookback2 <= 0 || lookback3 <= 0 || lookback4 <= 0)
            {
                throw new ArgumentOutOfRangeException("lookback");
            }

            _lookback1 = lookback1;
            _lookback2 = lookback2;
            _lookback3 = lookback3;
            _lookback4 = lookback4;
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] roc1 = new RateOfChange(_lookback1).Calculate(input[0]);
            double[] roc2 = new RateOfChange(_lookback2).Calculate(input[0]);
            double[] roc3 = new RateOfChange(_lookback3).Calculate(input[0]);
            double[] roc4 = new RateOfChange(_lookback4).Calculate(input[0]);

            double[] result = roc1.OperateThis(roc2, roc3, roc4,
                (r1, r2, r3, r4) => r1 + 2 * r2 + 3 * r3 + 4 * r4 / (1 + 2 + 3 + 4));

            return new double[1][] { result };
        }
    }
}
