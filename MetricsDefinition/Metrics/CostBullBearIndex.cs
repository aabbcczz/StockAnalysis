using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("CBBI")]
    public sealed class CostBullBearIndex : Metric
    {
        private int _lookback1;
        private int _lookback2;
        private int _lookback3;
        private int _lookback4;

        public CostBullBearIndex(int lookback1, int lookback2, int lookback3, int lookback4)
        {
            if (lookback1 <= 0 || lookback2 <= 0 || lookback3 <= 0 || lookback4 <=0)
            {
                throw new ArgumentOutOfRangeException("lookback must be greater than 0");
            }

            _lookback1 = lookback1;
            _lookback2 = lookback2;
            _lookback3 = lookback3;
            _lookback4 = lookback4;
        }

        public override double[][] Calculate(double[][] input)
        {
            double[] ma1 = new CostMovingAverage(_lookback1).Calculate(input[0]);
            double[] ma2 = new CostMovingAverage(_lookback2).Calculate(input[0]);
            double[] ma3 = new CostMovingAverage(_lookback3).Calculate(input[0]);
            double[] ma4 = new CostMovingAverage(_lookback4).Calculate(input[0]);

            double[] result = ma1.OperateThis(
                ma2, ma3, ma4,
                (m1, m2, m3, m4) => { return (m1 + m2 + m3 + m4) / 4; });

            return new double[1][] { result };
        }
    }
}
