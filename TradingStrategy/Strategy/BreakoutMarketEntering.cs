using System;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class BreakoutMarketEntering
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _metricProxy;

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

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _metricProxy = new RuntimeMetricProxy(Context.MetricManager, 
                string.Format(
                        "HI[{0}](BAR.{1})",
                        BreakoutWindow,
                        BarPriceSelector.GetSelectorString(PriceSelector)));
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            var values = _metricProxy.GetMetricValues(tradingObject);

            var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

            var price = BarPriceSelector.Select(bar, PriceSelector);

            var breakout = Math.Abs(values[0] - price) < 1e-6;

            if (breakout)
            {
                comments = string.Format("Breakout: {0:0.0000}", price);
            }

            return breakout;
        }
    }
}
