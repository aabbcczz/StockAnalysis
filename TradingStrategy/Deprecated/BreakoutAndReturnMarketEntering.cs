using System;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
{
    [DeprecatedStrategy]
    public sealed class BreakoutAndReturnMarketEntering 
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _metricProxy;

        public override string Name
        {
            get { return "通道突破折回入市"; }
        }

        public override string Description
        {
            get { return "当价格突破通道后然后在给定时间内折回到低点后入市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakoutWindow { get; set; }

        [Parameter(0, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        [Parameter(10, "通道突破后价格折回后再次上升所允许的最大间隔")]
        public int RerisingMaxInterval { get; set; }

        [Parameter(5, "通道突破后价格折回后再次上升所允许的最小间隔")]
        public int RerisingMinInterval { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (!BarPriceSelector.IsValidSelector(PriceSelector))
            {
                throw new ArgumentException("价格选择项非法");
            }

            if (RerisingMinInterval <= 0 || RerisingMaxInterval <= 0)
            {
                throw new ArgumentException("通道突破后价格折回后再次上升所允许的最大/最小时间间隔必须大于零");
            }

            if (RerisingMaxInterval > BreakoutWindow)
            {
                throw new ArgumentException("通道突破后价格折回后再次上升所允许的最大时间间隔必须小于等于通道突破窗口");
            }

            if (RerisingMinInterval > RerisingMaxInterval)
            {
                throw new ArgumentException("通道突破后价格折回后再次上升所允许的最小时间间隔必须小于等于最大时间间隔");
            }
        }

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            var metric = (BreakoutAndReturnRuntimeMetric)_metricProxy.GetMetric(tradingObject);
            if (metric.Triggered)
            {
                result.Comments = string.Format(
                    "Breakout: {0:0.0000}, LowestPrice: {1:0.0000}", 
                    metric.LatestBreakoutPrice,
                    metric.LowestPriceAfterBreakout);

                result.CanEnter = true;
            }

            return result;
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _metricProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                string.Format("BreakoutAndReturn[{0},{1},{2},{3}]", BreakoutWindow, PriceSelector, RerisingMaxInterval, RerisingMinInterval),
                (string s) => new BreakoutAndReturnRuntimeMetric(BreakoutWindow, PriceSelector, RerisingMaxInterval, RerisingMinInterval)); 
        }
    }
}
