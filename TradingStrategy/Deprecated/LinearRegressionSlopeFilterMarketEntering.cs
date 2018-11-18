using System;
using StockAnalysis.MetricsDefinition;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
{
    [DeprecatedStrategy]
    public sealed class LinearRegressionSlopeFilterMarketEntering 
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _longMetricProxy;
        private RuntimeMetricProxy _middleMetricProxy;
        private RuntimeMetricProxy _shortMetricProxy;

        [Parameter(70, "长期回看周期")]
        public int LongWindowSize { get; set; }

        [Parameter(35, "中期回看周期")]
        public int MiddleWindowSize { get; set; }

        [Parameter(15, "短期回看周期")]
        public int ShortWindowSize { get; set; }

        [Parameter(-90.0, "长期线性回归斜率角度阈值，取值为[-90.0..90.0]")]
        public double LongDegreeThreshold { get; set; }

        [Parameter(-90.0, "中期线性回归斜率角度阈值，取值为[-90.0..90.0]")]
        public double MiddleDegreeThreshold { get; set; }

        [Parameter(-90.0, "短期线性回归斜率角度阈值，取值为[-90.0..90.0]")]
        public double ShortDegreeThreshold { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _shortMetricProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("LR[{0}]", ShortWindowSize));
            _middleMetricProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("LR[{0}]", MiddleWindowSize));
            _longMetricProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("LR[{0}]", LongWindowSize));

        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (LongWindowSize <= 2 || MiddleWindowSize <= 2 || ShortWindowSize <= 2 )
            {
                throw new ArgumentException("windows size must be 0 or be greater than 2");
            }
        }

        public override string Name
        {
            get { return "线性回归斜率入市过滤器"; }
        }

        public override string Description
        {
            get { return "当长期，中期和短期线性回归斜率角度均大于相应阈值时允许入市"; }
        }

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            var longSlope = _longMetricProxy.GetMetricValues(tradingObject)[0];
            var longDegree = Math.Atan(longSlope) * 180.0 / Math.PI;
            var middleSlope = _middleMetricProxy.GetMetricValues(tradingObject)[0];
            var middleDegree = Math.Atan(middleSlope) * 180.0 / Math.PI;
            var shortSlope = _shortMetricProxy.GetMetricValues(tradingObject)[0];
            var shortDegree = Math.Atan(shortSlope) * 180.0 / Math.PI;

            if (longDegree > LongDegreeThreshold 
                && middleDegree > MiddleDegreeThreshold
                && shortDegree > ShortDegreeThreshold)
            {
                result.Comments = string.Format(
                    "LR Degree: L{0:0.000}, M{1:0.000}, S{2:0.000}",
                    longDegree,
                    middleDegree,
                    shortDegree);

                result.CanEnter = true;
            }

            return result;
        }
    }
}
