using System;

namespace TradingStrategy.Strategy
{
    public sealed class AtrTraceStopLossMarketExiting
        : GeneralTraceStopLossMarketExitingBase
    {
        private RuntimeMetricProxy _atrMetricProxy;

        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR停价倍数")]
        public double AtrStopLossFactor { get; set; }


        public override string Name
        {
            get { return "ATR跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "当价格向有利方向变动时，持续跟踪设置止损价为当前价格减去ATR乘以Atr停价倍数"; }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _atrMetricProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                string.Format("ATR[{0}]", AtrWindowSize));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (AtrWindowSize <= 1 || AtrStopLossFactor <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            var values = _atrMetricProxy.GetMetricValues(tradingObject);

            var atr = values[0];

            var stoploss = currentPrice - atr * AtrStopLossFactor;
            comments = string.Format(
                "stoploss({3:0.000}) = price({2:0.000}) - ATR({0:0.000}) * AtrStopLossFactor({1:0.000})",
                atr,
                AtrStopLossFactor,
                currentPrice,
                stoploss);

            return stoploss;
        }
    }
}
