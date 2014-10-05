using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class AtrDevStopLoss 
        : MetricBasedStopLossBase<AtrDevRuntimeMetric>
        , IStopLossComponent
    {
        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR标准差停价倍数")]
        public double AtrDevStopLossFactor { get; set; }

        [Parameter(10.0, "ATR标准差调整百分比")]
        public double AdjustmentPercentage { get; set; }

        public override string Name
        {
            get { return "ATR标准差停价"; }
        }

        public override string Description
        {
            get { return "当价格低于买入价，并且差值>ATR的标准差*ATR标准差停价倍数*(1+ATR标准差调整百分比/100)时停价"; }
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

        public override double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments)
        {
            AtrDevRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            comments = string.Format(
                "stoplossgap = STDDEV_ATR({0:0.000}) * AtrDevStopLossFactor({1:0.000}) * (1.0 + AdjustmentPercentage({2:0.000})) / 100.0",
                metric.Sdtr,
                AtrDevStopLossFactor,
                AdjustmentPercentage);

            return -metric.Sdtr * AtrDevStopLossFactor * (1.0 + AdjustmentPercentage / 100.0);
        }
    }
}
