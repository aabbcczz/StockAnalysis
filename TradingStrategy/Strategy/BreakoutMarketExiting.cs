using System;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class BreakoutMarketExiting
        : MetricBasedMarketExiting
    {
        public override string Name
        {
            get { return "通道突破退市"; }
        }

        public override string Description
        {
            get { return "当价格突破通道后退市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakoutWindow { get; set; }

        [Parameter(1, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        [Parameter(1, "突破方向。0为低位突破，1为高位突破")]
        public int BreakoutDirection { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (BreakoutDirection != 0 && BreakoutDirection != 1)
            {
                throw new ArgumentException("BreakoutDirection can be only 0 or 1");
            }
        }
        
        protected override IMetricBooleanExpression BuildExpression()
        {
            return new Comparison(
                string.Format(
                    "{2}[{0}](BAR.{1}) == BAR.{1}",
                    BreakoutWindow,
                    BarPriceSelector.GetSelectorString(PriceSelector),
                    BreakoutDirection == 0 ? "LO" : "HI"));
        }
    }
}