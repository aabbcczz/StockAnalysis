using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("CCI")]
    public sealed class CommodityChannelIndex : SingleOutputBarInputSerialMetric
    {
        private const double Alpha = 0.015;
        private MovingAverage _maTruePrice;
        private CirculatedArray<double> _truePrices;

        public CommodityChannelIndex(int windowSize)
            : base(1)
        {
            _maTruePrice = new MovingAverage(windowSize);
            _truePrices = new CirculatedArray<double>(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            double truePrice = (bar.HighestPrice + bar.LowestPrice + 2 * bar.ClosePrice ) / 4;
            _truePrices.Add(truePrice);

            double maTruePrice = _maTruePrice.Update(truePrice);

            double sum = 0.0;
            for (int i = 0; i < _truePrices.Length; ++i)
            {
                sum += Math.Abs(_truePrices[i] - maTruePrice);
            }

            double d = sum / _truePrices.Length;

            return d == 0.0 ? 0.0 : (truePrice - maTruePrice) / d / Alpha;
        }
    }
}
