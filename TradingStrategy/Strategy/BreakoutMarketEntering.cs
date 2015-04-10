using System;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class BreakoutMarketEntering
        : MetricBasedMarketEntering
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
        public int BreakoutWindow { get; set; }

        [Parameter(0, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        protected override IMetricBooleanExpression BuildExpression()
        {
            return new Comparison(
                string.Format(
                    "HI[{0}](BAR.{1}) == BAR.{1}",
                    BreakoutWindow,
                    BarPriceSelector.GetSelectorString(PriceSelector)));

        }
    }
}
