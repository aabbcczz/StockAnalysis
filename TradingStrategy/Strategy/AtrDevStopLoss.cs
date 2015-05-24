using System;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class AtrDevStopLoss
        : GeneralStopLossBase
    {
        private RuntimeMetricProxy _sdtrMetricProxy;

        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR标准差停价倍数")]
        public double AtrDevStopLossFactor { get; set; }

        public override string Name
        {
            get { return "ATR标准差停价"; }
        }

        public override string Description
        {
            get { return "当价格低于买入价，并且差值>ATR的标准差*ATR标准差停价倍数时停价"; }
        }

        public override bool DoesStopLossGapDependsOnPrice
        {
            get
            {
                return true;
            }
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

        public override StopLossComponentResult EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice)
        {
            var values = _sdtrMetricProxy.GetMetricValues(tradingObject);

            var sdtr = values[0];

            var stoplossGap = -sdtr * AtrDevStopLossFactor;

            var comments = string.Format(
                "stoplossgap({2:0.000}) = STDEV_ATR({0:0.000}) * AtrDevStopLossFactor({1:0.000})",
                sdtr,
                AtrDevStopLossFactor,
                stoplossGap);

            return new StopLossComponentResult()
                {
                    Comments = comments,
                    StopLossGap = stoplossGap
                };
        }
    }
}
