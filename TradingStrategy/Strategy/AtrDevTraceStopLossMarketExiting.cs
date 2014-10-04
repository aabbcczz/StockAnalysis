using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class AtrDevTraceStopLossMarketExiting
        : MetricBasedTraceStopLossMarketExiting<AtrDevRuntimeMetric>
    {
        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR标准差停价倍数")]
        public double AtrDevStopLossFactor { get; set; }

        [Parameter(10.0, "ATR标准差调整百分比")]
        public double AdjustmentPercentage { get; set; }

        public override string Name
        {
            get { return "ATR标准差跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "当价格向有利方向变动时，持续跟踪设置止损价为当前价格减去ATR的标准差*ATR标准差停价倍数*(1+ATR标准差调整百分比/100)"; }
        }

        public override Func<AtrDevRuntimeMetric> Creator
        {
            get { return (() => { return new AtrDevRuntimeMetric(AtrWindowSize); }); }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (AtrWindowSize <= 1 || AtrDevStopLossFactor <= 0.0 || AdjustmentPercentage < 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice)
        {
            AtrDevRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);
            return currentPrice - metric.Atr * AtrDevStopLossFactor * (1.0 + AdjustmentPercentage / 100.0);
        }
    }
}
