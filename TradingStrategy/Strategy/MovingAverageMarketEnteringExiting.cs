using System;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageMarketEnteringExiting
        : MetricBasedMarketEnteringBase<GenericRuntimeMetric>
        , IMarketExitingComponent
    {
        [Parameter(5, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public int Long { get; set; }

        protected override Func<GenericRuntimeMetric> Creator
        {
            get { return (() => new GenericRuntimeMetric(string.Format("MA[{0}];MA[{1}]", Short, Long), true)); }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (Short >= Long)
            {
                throw new ArgumentException("Short parameter value must be smaller than Long parameter value");
            }
        }

        public override string Name
        {
            get { return "移动平均入市出市"; }
        }

        public override string Description
        {
            get { return "当短期平均向上交叉长期平均时入市，当短期平均向下交叉长期平均时出市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var runtimeMetric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var shortMa = runtimeMetric.LatestData[0][0];
            var longMa = runtimeMetric.LatestData[1][0];
            var prevShortMa = runtimeMetric.PreviousData[0][0];
            var prevLongMa = runtimeMetric.PreviousData[1][0];

            if (shortMa > longMa && prevShortMa < prevLongMa)
            {
                comments = string.Format(
                    "prevShort:{0:0.000}; prevLong:{1:0.000}; curShort:{2:0.000}; curLong:{3:0.000}",
                    prevShortMa,
                    prevLongMa,
                    shortMa,
                    longMa);

                return true;
            }

            return false;
        }

        public bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var runtimeMetric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var shortMa = runtimeMetric.LatestData[0][0];
            var longMa = runtimeMetric.LatestData[1][0];
            var prevShortMa = runtimeMetric.PreviousData[0][0];
            var prevLongMa = runtimeMetric.PreviousData[1][0];

            if (shortMa < longMa && prevShortMa > prevLongMa)
            {
                comments = string.Format(
                    "prevShort:{0:0.000}; prevLong:{1:0.000}; curShort:{2:0.000}; curLong:{3:0.000}",
                    prevShortMa,
                    prevLongMa,
                    shortMa,
                    longMa);

                return true;
            }

            return false;
        }
    }
}
