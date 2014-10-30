using System;

namespace TradingStrategy.Strategy
{
    public sealed class SarTraceStopLossMarketExiting
        : MetricBasedTraceStopLossMarketExiting<GenericRuntimeMetric>
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

        protected override Func<GenericRuntimeMetric> Creator
        {
            get { return (() => new GenericRuntimeMetric("SAR[4,0.02,0.02,0.2]")); }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MaxPercentageOfPrice < 0.0 || MaxPercentageOfPrice > 100.0)
            {
                throw new ArgumentOutOfRangeException("MaxPercentageOfPrice is not in [0.0..100.0]");
            }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var sar = metric.LatestData[0][0];

            comments = string.Format(
                "stoploss = Min(Price({0:0.000}) * MaxPercentageOfPrice({1:0.000}) / 100.0, SAR({2:0.000}))",
                currentPrice,
                MaxPercentageOfPrice,
                sar);

            return Math.Min(currentPrice * MaxPercentageOfPrice / 100.0, sar);
        }
    }
}
