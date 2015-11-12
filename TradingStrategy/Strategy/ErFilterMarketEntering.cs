using System;
using MetricsDefinition;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class ErFilterMarketEntering 
        : MetricBasedMarketEntering
    {
        [Parameter(10, "EfficiencyRatio周期")]
        public int ErWindowSize { get; set; }

        [Parameter(0.8, "EfficiencyRatio阈值")]
        public double ErThreshold { get; set; }

        protected override IMetricBooleanExpression BuildExpression()
        {
            return new Comparison(
                string.Format("ER[{0}] > {1:0.000}", ErWindowSize, ErThreshold));
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
    }
}
