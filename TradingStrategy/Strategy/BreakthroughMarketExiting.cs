using System;

namespace TradingStrategy.Strategy
{
    public sealed class BreakthroughMarketExiting
        : MetricBasedMarketExitingBase<LowBreakthroughRuntimeMetric>
    {
        public override string Name
        {
            get { return "通道突破退市"; }
        }

        public override string Description
        {
            get { return "当最低价格突破通道后退市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakthroughWindow { get; set; }

        protected override Func<LowBreakthroughRuntimeMetric> Creator
        {
            get { return (() => new LowBreakthroughRuntimeMetric(BreakthroughWindow)); }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            LowBreakthroughRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            if (metric.Breakthrough)
            {
                comments = string.Format("Breakthrough: {0:0.0000}", metric.CurrentLowest);
            }

            return metric.Breakthrough;
        }
    }
}