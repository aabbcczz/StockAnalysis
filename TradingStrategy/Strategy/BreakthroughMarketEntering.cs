using System;

namespace TradingStrategy.Strategy
{
    public sealed class BreakthroughMarketEntering
        : MetricBasedMarketEnteringBase<GenericRuntimeMetric>
    {
        public override string Name
        {
            get { return "通道突破入市"; }
        }

        public override string Description
        {
            get { return "当最高价格突破通道后入市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakthroughWindow { get; set; }

        [Parameter(0, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

            var price = BarPriceSelector.Select(bar, PriceSelector);

            var breakthrough = Math.Abs(metric.LatestData[0][0] - price) < 1e-6;

            if (breakthrough)
            {
                comments = string.Format("Breakthrough: {0:0.0000}", price);
            }

            return breakthrough;
        }

        protected override Func<GenericRuntimeMetric> Creator
        {
            get 
            {
                return (() => new GenericRuntimeMetric(
                    string.Format(
                        "HI[{0}](BAR.{1})", 
                        BreakthroughWindow, 
                        BarPriceSelector.GetSelectorString(PriceSelector))));    
            }
        }
    }
}
