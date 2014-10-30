using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("UOS")]
    public sealed class UltimateOscillator : SingleOutputBarInputSerialMetric
    {
        private readonly MovingSum[] _msBp = new MovingSum[3];
        private readonly MovingSum[] _msTr = new MovingSum[3];
        private readonly double[] _weight = new double[3];
        private double _prevClosePrice;
        public UltimateOscillator(int windowSize1, int windowSize2, int windowSize3, double weight1, double weight2, double weight3)
            : base(0)
        {
            if (weight1 < 0.0 || weight2 < 0.0 || weight3 < 0.0)
            {
                throw new ArgumentOutOfRangeException("weight");
            }

            _weight[0] = weight1;
            _weight[1] = weight2;
            _weight[2] = weight3;

            _msBp[0] = new MovingSum(windowSize1);
            _msTr[0] = new MovingSum(windowSize1);
            _msBp[1] = new MovingSum(windowSize2);
            _msTr[1] = new MovingSum(windowSize2);
            _msBp[2] = new MovingSum(windowSize3);
            _msTr[2] = new MovingSum(windowSize3);
        }

        public override double Update(Bar bar)
        {
            var bp = bar.ClosePrice - Math.Min(bar.LowestPrice, _prevClosePrice);
            var tr = Math.Max(bar.HighestPrice, _prevClosePrice) - Math.Min(bar.LowestPrice, _prevClosePrice);

            var average = new double[3];

            for (var i = 0; i < 3; ++i)
            {
                average[i] = _msBp[i].Update(bp) / _msTr[i].Update(tr);
            }

            var sumWeight = (_weight[0] + _weight[1] + _weight[2]) / 100.0;

            var result = (_weight[0] * average[0] 
                + _weight[1] * average[1] 
                + _weight[2] * average[2])
                / sumWeight;

            // update status;
            _prevClosePrice = bar.ClosePrice;

            return result;
        }
    }
}
