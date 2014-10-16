using System;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageMarketExiting 
        : MetricBasedMarketExitingBase<MovingAverageRuntimeMetric>
    {
        [Parameter(5, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public int Long { get; set; }

        public override Func<MovingAverageRuntimeMetric> Creator
        {
            get { return (() => new MovingAverageRuntimeMetric(Short, Long)); }
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
            get { return "移动平均出市"; }
        }

        public override string Description
        {
            get { return "当短期平均向下交叉长期平均时出市"; }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var runtimeMetric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            if (!(runtimeMetric.ShortMa < runtimeMetric.LongMa) ||
                !(runtimeMetric.PreviousShortMa > runtimeMetric.PreviousLongMa))
            {
                return false;
            }

            comments = string.Format(
                "prevShort:{0:0.000}; prevLong:{1:0.000}; curShort:{2:0.000}; curLong:{3:0.000}",
                runtimeMetric.PreviousShortMa,
                runtimeMetric.PreviousLongMa,
                runtimeMetric.ShortMa,
                runtimeMetric.LongMa);

            return true;
        }
    }
}
