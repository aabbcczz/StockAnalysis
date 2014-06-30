using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using System.Reflection;

namespace MetricsDefinition
{
    /// <summary>
    /// The Chavkin metric
    /// </summary>
    [Metric("CV")]
    public sealed class Chavkin : SingleOutputBarInputSerialMetric
    {
        private int _interval;
        private ExponentialMovingAverage _ema;
        private CirculatedArray<double> _mahl;

        public Chavkin(int windowSize, int interval)
            : base(1)
        {
            if (interval <= 0 || interval > windowSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            _interval = interval;

            _ema = new ExponentialMovingAverage(windowSize);
            _mahl = new CirculatedArray<double>(interval);
        }

        public override double Update(Bar bar)
        {
            double mahl = _ema.Update(bar.HighestPrice - bar.LowestPrice);

            _mahl.Add(mahl);

            if (_mahl[0] == 0.0)
            {
                return 0.0;
            }
            else
            {
                return (_mahl[_mahl.Length - 1] - _mahl[0]) / _mahl[0] * 100.0;
            }

        }
    }
}
