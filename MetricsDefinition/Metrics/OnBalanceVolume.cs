using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("OBV")]
    public sealed class OnBalanceVolume : SingleOutputBarInputSerialMetric
    {
        private double _prevObv = 0.0;
        private double _prevClosePrice = 0.0;
        private bool _firstBar = true;

        public OnBalanceVolume()
            : base(1)
        {
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            double obv = _firstBar
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
