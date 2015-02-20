using System;

namespace TradingStrategy.Strategy
{
    public sealed class BounceMarketEntering
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _bounceMetricProxy;

        [Parameter(30, "回看周期")]
        public int WindowSize { get; set; }

        [Parameter(5.0, "反弹百分比")]
        public double MinBouncePercentage { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _bounceMetricProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                string.Format("BounceRuntimeMetric[{0},{1}]", WindowSize, MinBouncePercentage),
                (string s) => new BounceRuntimeMetric(WindowSize, MinBouncePercentage));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MinBouncePercentage <= 0.0)
            {
                throw new ArgumentException("MinBouncePercentage must be greater than 0.0");
            }
        }

        public override string Name
        {
            get { return "低点反弹入市"; }
        }

        public override string Description
        {
            get { return "当价格达到回看周期(WindowSize)内最低点后然后反弹超过最小反弹百分比（MinBouncePercentage）时入市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            var metric = (BounceRuntimeMetric)_bounceMetricProxy.GetMetric(tradingObject);
            if (metric.Triggered)
            {
                comments = string.Format(
                    "Lowest:{0:0.000}; Current:{1:0.000}; BouncePercentage:{2:0.000}%",
                    metric.LowestPrice,
                    metric.BouncePrice,
                    metric.BouncePercentage);

                return true;
            }

            return false;
        }
    }
}
