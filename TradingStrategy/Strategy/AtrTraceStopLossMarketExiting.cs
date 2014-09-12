using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class AtrTraceStopLossMarketExiting 
        : MetricBasedTraceStopLossMarketExiting<AtrRuntimeMetric>
    {
        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR停价倍数")]
        public double AtrStopLossFactor { get; set; }


        public override string Name
        {
            get { return "ATR跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "当价格向有利方向变动时，持续跟踪设置止损价为当前价格减去ATR乘以Atr停价倍数"; }
        }

        public override Func<AtrRuntimeMetric> Creator
        {
            get { return (() => { return new AtrRuntimeMetric(AtrWindowSize); }); }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice)
        {
            AtrRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            return currentPrice - metric.Atr * AtrStopLossFactor;
        }
    }
}
