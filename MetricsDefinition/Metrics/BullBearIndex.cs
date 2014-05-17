using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("BBI")]
    class BullBearIndex : IMetric
    {
        private int _lookback1;
        private int _lookback2;
        private int _lookback3;
        private int _lookback4;

        public BullBearIndex(int lookback1, int lookback2, int lookback3, int lookback4)
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

        public IEnumerable<double>[] Calculate(IEnumerable<double>[] input)
        {
            double[] ma1 = new MovingAverage(_lookback1).Calculate(input).First().ToArray();
            double[] ma2 = new MovingAverage(_lookback2).Calculate(input).First().ToArray();
            double[] ma3 = new MovingAverage(_lookback3).Calculate(input).First().ToArray();
            double[] ma4 = new MovingAverage(_lookback4).Calculate(input).First().ToArray();

            double[] result = new double[ma1.Length];
            for (int i = 0; i < ma1.Length; ++i)
            {
                result[i] = (ma1[i] + ma2[i] + ma3[i] + ma4[i] ) / 4;
            }

            return new IEnumerable<double>[1] { result };
        }
    }
}
