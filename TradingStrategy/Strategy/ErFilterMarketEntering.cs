using System;
using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class ErFilterMarketEntering 
        : MetricBasedMarketEnteringBase<GenericRuntimeMetric>
    {
        [Parameter(10, "EfficiencyRatio周期")]
        public int ErWindowSize { get; set; }

        [Parameter(0.8, "EfficiencyRatio阈值")]
        public double ErThreshold { get; set; }

        protected override Func<GenericRuntimeMetric> Creator
        {
            get { return (() => new GenericRuntimeMetric(string.Format("ER[{0}]", ErWindowSize))); }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (ErWindowSize <= 0)
            {
                throw new ArgumentException("ER windows size must be greater than 0");
            }

            if (ErThreshold < 0.0 || ErThreshold > 1.0)
            {
                throw new ArgumentOutOfRangeException("ER threshold must be in [0.0..1.0]");
            }
        }

        public override string Name
        {
            get { return "EfficiencyRatio入市过滤器"; }
        }

        public override string Description
        {
            get { return "当EfficiencyRatio超过ErThreshold时允许入市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var runtimeMetric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var efficiencyRatio = runtimeMetric.LatestData[0][0];

            if (efficiencyRatio > ErThreshold)
            {
                comments = string.Format(
                    "ER:{0:0.000}",
                    efficiencyRatio);

                return true;
            }

            return false;
        }
    }
}
