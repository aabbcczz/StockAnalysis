using System;

namespace TradingStrategy.Strategy
{
    public sealed class RebreakoutMarketEntering 
        : GeneralMarketEnteringBase
    {
        private int _metricIndex;

        public override string Name
        {
            get { return "通道再次突破入市"; }
        }

        public override string Description
        {
            get { return "当最高价格突破通道后，价格U型反转后再次突破上次最高价格后入市"; }
        }

        [Parameter(20, "通道突破窗口")]
        public int BreakoutWindow { get; set; }

        [Parameter(0, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        [Parameter(10, "通道再次突破允许的最大间隔")]
        public int RebreakoutMaxInterval { get; set; }

        [Parameter(5, "通道再次突破允许的最小间隔")]
        public int RebreakoutMinInterval { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (!BarPriceSelector.IsValidSelector(PriceSelector))
            {
                throw new ArgumentException("价格选择项非法");
            }

            if (RebreakoutMinInterval <=0 || RebreakoutMaxInterval <= 0)
            {
                throw new ArgumentException("再突破最大/最小时间间隔必须大于零");
            }

            if (RebreakoutMaxInterval > BreakoutWindow)
            {
                throw new ArgumentException("再突破最大时间间隔必须小于等于通道突破窗口");
            }

            if (RebreakoutMinInterval > RebreakoutMaxInterval)
            {
                throw new ArgumentException("再突破最小时间间隔必须小于等于最大时间间隔");
            }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var metric = (RebreakoutRuntimeMetric)Context.MetricManager.GetMetric(tradingObject, _metricIndex);
            if (metric.Rebreakout)
            {
                comments = string.Format("Rebreakout: {0:0.0000}, Interval: {1}", metric.CurrentHighest, metric.IntervalSinceLastBreakout);
            }

            return metric.Rebreakout;
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _metricIndex = Context.MetricManager.RegisterMetric(
                string.Format("Rebreakout[{0},{1},{2},{3}]",
                    BreakoutWindow,
                    PriceSelector,
                    RebreakoutMaxInterval,
                    RebreakoutMinInterval),
                (string s) => new RebreakoutRuntimeMetric(
                    BreakoutWindow, 
                    PriceSelector,
                    RebreakoutMaxInterval,
                    RebreakoutMinInterval));
        }
    }
}
