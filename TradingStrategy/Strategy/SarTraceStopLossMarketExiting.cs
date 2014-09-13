using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class SarTraceStopLossMarketExiting 
        : MetricBasedTraceStopLossMarketExiting<SarRuntimeMetric>
    {
        [Parameter(95.0, "SAR占价格的最大百分比")]
        public double MaxPercentageOfPrice { get; set; }


        public override string Name
        {
            get { return "SAR跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "当价格向有利方向变动时，持续跟踪设置止损价为SAR值，并不超过当期价格*MaxPercentageOfPrice/100.0"; }
        }

        public override Func<SarRuntimeMetric> Creator
        {
            get { return (() => { return new SarRuntimeMetric(); }); }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MaxPercentageOfPrice < 0.0 || MaxPercentageOfPrice > 100.0)
            {
                throw new ArgumentOutOfRangeException("MaxPercentageOfPrice is not in [0.0..100.0]");
            }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice)
        {
            SarRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            return Math.Min(currentPrice * MaxPercentageOfPrice / 100, metric.Sar);
        }
    }
}
