using System;

namespace TradingStrategy.Strategy
{
    public sealed class AtrDevTraceStopLossMarketExiting
        : GeneralTraceStopLossMarketExitingBase
    {
        private RuntimeMetricProxy _sdtrMetricProxy;

        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR标准差停价倍数")]
        public double AtrDevStopLossFactor { get; set; }

        public override string Name
        {
            get { return "ATR标准差跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "当价格向有利方向变动时，持续跟踪设置止损价为当前价格减去ATR的标准差*ATR标准差停价倍数"; }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();
            _sdtrMetricProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                string.Format("SDTR[{0}]", AtrWindowSize));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (AtrWindowSize <= 1 || AtrDevStopLossFactor <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            var values = _sdtrMetricProxy.GetMetricValues(tradingObject);

            var sdtr = values[0];

            var stoploss = currentPrice - sdtr * AtrDevStopLossFactor;
            comments = string.Format(
                "stoploss({3:0.000}) = Price({2:0.000}) - STDEV_ATR({0:0.000}) * AtrDevStopLossFactor({1:0.000})",
                sdtr,
                AtrDevStopLossFactor,
                currentPrice,
                stoploss);

            return stoploss;
        }
    }
}
