using System;
using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class LinearRegressionSlopeFilterMarketEntering 
        : MetricBasedMarketEnteringBase<GenericRuntimeMetric>
    {
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

        protected override Func<GenericRuntimeMetric> Creator
        {
            get { return (() => new GenericRuntimeMetric(string.Format("LR[{0}];LR[{1}];LR[{2}]", LongWindowSize, MiddleWindowSize, ShortWindowSize))); }
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

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var runtimeMetric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var longSlope = runtimeMetric.LatestData[0][0];
            var longDegree = Math.Atan(longSlope) * 180.0 / Math.PI;
            var middleSlope = runtimeMetric.LatestData[1][0];
            var middleDegree = Math.Atan(middleSlope) * 180.0 / Math.PI;
            var shortSlope = runtimeMetric.LatestData[2][0];
            var shortDegree = Math.Atan(shortSlope) * 180.0 / Math.PI;

            if (longDegree > LongDegreeThreshold 
                && middleDegree > MiddleDegreeThreshold
                && shortDegree > ShortDegreeThreshold)
            {
                comments = string.Format(
                    "LR Degree: L{0:0.000}, M{1:0.000}, S{2:0.000}",
                    longDegree,
                    middleDegree,
                    shortDegree);

                return true;
            }

            return false;
        }
    }
}
