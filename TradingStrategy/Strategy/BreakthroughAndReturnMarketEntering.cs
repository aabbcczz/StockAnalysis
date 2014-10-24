using System;

namespace TradingStrategy.Strategy
{
    public sealed class BreakthroughAndReturnMarketEntering 
        : MetricBasedMarketEnteringBase<BreakthroughAndReturnRuntimeMetric>
    {
        public override string Name
        {
            get { return "通道突破折回入市"; }
        }

        public override string Description
        {
            get { return "当最高价格突破通道后然后在给定时间内折回到低点后入市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakthroughWindow { get; set; }

        [Parameter(10, "通道突破后价格折回后再次上升所允许的最大间隔")]
        public int RerisingMaxInterval { get; set; }

        [Parameter(5, "通道突破后价格折回后再次上升所允许的最小间隔")]
        public int RerisingMinInterval { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (RerisingMinInterval <= 0 || RerisingMaxInterval <= 0)
            {
                throw new ArgumentException("通道突破后价格折回后再次上升所允许的最大/最小时间间隔必须大于零");
            }

            if (RerisingMaxInterval > BreakthroughWindow)
            {
                throw new ArgumentException("通道突破后价格折回后再次上升所允许的最大时间间隔必须小于等于通道突破窗口");
            }

            if (RerisingMinInterval > RerisingMaxInterval)
            {
                throw new ArgumentException("通道突破后价格折回后再次上升所允许的最小时间间隔必须小于等于最大时间间隔");
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            if (metric.Triggered)
            {
                comments = string.Format("Breakthrough: {0:0.0000}, LowestPrice: {1:0.0000}", metric.LatestBreakthroughPrice, metric.LowestPriceAfterBreakthrough);
            }

            return metric.Triggered;
        }

        protected override Func<BreakthroughAndReturnRuntimeMetric> Creator
        {
            get 
            {
                return (() => new BreakthroughAndReturnRuntimeMetric(BreakthroughWindow, RerisingMaxInterval, RerisingMinInterval));    
            }
        }
    }
}
