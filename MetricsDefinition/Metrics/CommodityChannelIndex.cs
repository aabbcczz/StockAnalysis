using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("CCI")]
    public sealed class CommodityChannelIndex : SingleOutputBarInputSerialMetric
    {
        private const double Alpha = 0.015;
        private readonly MovingAverage _maTruePrice;
        private readonly CirculatedArray<double> _truePrices;

        public CommodityChannelIndex(int windowSize)
            : base(0)
        {
            _maTruePrice = new MovingAverage(windowSize);
            _truePrices = new CirculatedArray<double>(windowSize);
        }

        public override double Update(Bar bar)
        {
            var truePrice = (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice ) / 4;
            _truePrices.Add(truePrice);

            var maTruePrice = _maTruePrice.Update(truePrice);

            var sum = 0.0;
            for (var i = 0; i < _truePrices.Length; ++i)
            {
                sum += Math.Abs(_truePrices[i] - maTruePrice);
            }

            var d = sum / _truePrices.Length;

            return Math.Abs(d) < 1e-6 ? 0.0 : (truePrice - maTruePrice) / d / Alpha;
        }
    }
}
