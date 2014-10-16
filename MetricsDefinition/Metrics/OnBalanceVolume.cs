using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("OBV")]
    public sealed class OnBalanceVolume : SingleOutputBarInputSerialMetric
    {
        private double _prevObv;
        private double _prevClosePrice;
        private bool _firstBar = true;

        public OnBalanceVolume()
            : base(1)
        {
        }

        public override double Update(Bar bar)
        {
            var obv = _firstBar
                ? bar.Volume
                : _prevObv + Math.Sign(bar.ClosePrice - _prevClosePrice) * bar.Volume;

            // update status
            _prevClosePrice = bar.ClosePrice;
            _prevObv = obv;
            _firstBar = false;

            // return result;
            return obv;
        }
    }
}
