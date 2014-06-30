using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("KDJ", "K,D,J")]
    public sealed class KDJStochastics : MultipleOutputBarInputSerialMetric
    {
        private int _kWindowSize;
        private int _kDecay;
        private int _jCoeff;

        private double _prevK = 50.0;
        private double _prevD = 50.0;

        private Highest _highest;
        private Lowest _lowest;

        public KDJStochastics(int kWindowSize, int kDecay, int jCoeff)
            : base(1)
        {
            if (kWindowSize <= 0 || kDecay <= 0 || jCoeff <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _kWindowSize = kWindowSize;
            _kDecay = kDecay;
            _jCoeff = jCoeff;

            _highest = new Highest(kWindowSize);
            _lowest = new Lowest(kWindowSize);
        }

        public override double[] Update(StockAnalysis.Share.Bar bar)
        {
            double lowestPrice = _lowest.Update(bar.LowestPrice);
            double highestPrice = _highest.Update(bar.HighestPrice);

            double rsv = (bar.ClosePrice - lowestPrice) / (highestPrice - lowestPrice) * 100.0;

            double k = ((_kDecay - 1) * _prevK + rsv ) / _kDecay;
            double d = ((_kDecay - 1) * _prevD + k) / _kDecay;
            double j = _jCoeff * d - (_jCoeff - 1) * k;

            // update status;
            _prevK = k;
            _prevD = d;

            return new double[3]{ k, d, j};

        }
    }
}
