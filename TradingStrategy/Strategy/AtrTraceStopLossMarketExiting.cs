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

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (AtrWindowSize <= 1 || AtrStopLossFactor <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            AtrRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            comments = string.Format(
                "stoploss = price({2:0.000}) - ATR({0:0.000}) * AtrStopLossFactor({1:0.000})",
                metric.Atr,
                AtrStopLossFactor,
                currentPrice);

            return currentPrice - metric.Atr * AtrStopLossFactor;
        }
    }
}
