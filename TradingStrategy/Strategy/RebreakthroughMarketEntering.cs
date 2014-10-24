using System;

namespace TradingStrategy.Strategy
{
    public sealed class RebreakthroughMarketEntering 
        : MetricBasedMarketEnteringBase<RebreakthroughRuntimeMetric>
    {
        public override string Name
        {
            get { return "通道再次突破入市"; }
        }

        public override string Description
        {
            get { return "当最高价格突破通道后，价格U型反转后再次突破上次最高价格后入市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakthroughWindow { get; set; }

        [Parameter(10, "通道再次突破允许的最大间隔")]
        public int RebreakthroughMaxInterval { get; set; }

        [Parameter(5, "通道再次突破允许的最小间隔")]
        public int RebreakthroughMinInterval { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (RebreakthroughMinInterval <=0 || RebreakthroughMaxInterval <= 0)
            {
                throw new ArgumentException("再突破最大/最小时间间隔必须大于零");
            }

            if (RebreakthroughMaxInterval > BreakthroughWindow)
            {
                throw new ArgumentException("再突破最大时间间隔必须小于等于通道突破窗口");
            }

            if (RebreakthroughMinInterval > RebreakthroughMaxInterval)
            {
                throw new ArgumentException("再突破最小时间间隔必须小于等于最大时间间隔");
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            if (metric.Rebreakthrough)
            {
                comments = string.Format("Rebreakthrough: {0:0.0000}, Interval: {1}", metric.CurrentHighest, metric.IntervalSinceLastBreakthrough);
            }

            return metric.Rebreakthrough;
        }

        protected override Func<RebreakthroughRuntimeMetric> Creator
        {
            get 
            {
                return (() => new RebreakthroughRuntimeMetric(
                    BreakthroughWindow, 
                    RebreakthroughMaxInterval,
                    RebreakthroughMinInterval));    
            }
        }
    }
}
