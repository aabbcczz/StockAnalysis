using System;

namespace TradingStrategy.Strategy
{
    public sealed class BreakthroughMarketExiting
        : MetricBasedMarketExitingBase<GenericRuntimeMetric>
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

        [Parameter(1, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        protected override Func<GenericRuntimeMetric> Creator
        {
            get
            {
                return (() => new GenericRuntimeMetric(
                    string.Format(
                    "LO[{0}](BAR.{1})",
                    BreakthroughWindow,
                    BarPriceSelector.GetSelectorString(PriceSelector))));
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

            var price = BarPriceSelector.Select(bar, PriceSelector);

            bool breakthough = Math.Abs(price - metric.LatestData[0][0]) < 1e-6;

            if (breakthough)
            {
                comments = string.Format("Breakthrough: {0:0.0000}", price);
            }

            return breakthough;
        }
    }
}