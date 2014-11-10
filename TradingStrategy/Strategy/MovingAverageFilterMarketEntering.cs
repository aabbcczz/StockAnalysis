using System;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageFilterMarketEntering
        : GeneralMarketEnteringBase
    {
        private int _shortMetricIndex;
        private int _longMetricIndex;

        [Parameter(55, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(300, "长期移动平均周期")]
        public int Long { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();
            _shortMetricIndex = Context.MetricManager.RegisterMetric(string.Format("MA[{0}]", Short));
            _longMetricIndex = Context.MetricManager.RegisterMetric(string.Format("MA[{0}]", Long));
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
            get { return "移动平均入市过滤器"; }
        }

        public override string Description
        {
            get { return "当短期平均在长期平均上方时允许入市"; }
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;
            var shortMa = Context.MetricManager.GetMetricValues(tradingObject, _shortMetricIndex)[0];
            var longMa = Context.MetricManager.GetMetricValues(tradingObject, _longMetricIndex)[0];

            if (shortMa > longMa)
            {
                comments = string.Format(
                    "Short:{0:0.000}; Long:{1:0.000}",
                    shortMa,
                    longMa);

                return true;
            }

            return false;
        }
    }
}
