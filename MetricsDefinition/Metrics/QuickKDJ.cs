using System;
using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("QKD", "K,D,J")]
    public sealed class QuickKdj : MultipleOutputBarInputSerialMetric
    {
        private readonly int _kDecay;
        private readonly int _jCoeff;

        private double _prevD = 50.0;

        private readonly Highest _highest;
        private readonly Lowest _lowest;

        public QuickKdj(int kWindowSize, int kDecay, int jCoeff)
            : base(0)
        {
            if (kWindowSize <= 0 || kDecay <= 0 || jCoeff <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _kDecay = kDecay;
            _jCoeff = jCoeff;

            _highest = new Highest(kWindowSize);
            _lowest = new Lowest(kWindowSize);

            Values = new double[3];
        }

        public override void Update(Bar bar)
        {
            _lowest.Update(bar.LowestPrice);
            var lowestPrice = _lowest.Value;

            _highest.Update(bar.HighestPrice);
            var highestPrice = _highest.Value;

            var rsv = (bar.ClosePrice - lowestPrice) / (highestPrice - lowestPrice) * 100.0;

            var k = rsv;
            var d = ((_kDecay - 1) * _prevD + k) / _kDecay;
            var j = _jCoeff * d - (_jCoeff - 1) * k;

            // update status;
            _prevD = d;

            SetValue(k, d, j);
        }
    }
}
