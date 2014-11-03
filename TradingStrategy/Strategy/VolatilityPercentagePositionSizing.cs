using System;

namespace TradingStrategy.Strategy
{
    public sealed class VolatilityPercentagePositionSizing
        : MetricBasedPositionSizingBase<GenericRuntimeMetric>
    {
        private EquityEvaluationMethod _equityEvaluationMethod;

        [Parameter(10, "波动率计算时间窗口大小")]
        public int VolatilityWindowSize { get; set; }

        [Parameter(1.0, "每份头寸的波动率占权益的百分比")]
        public double PercentageOfEquityForEachPositionVolatility { get; set; }

        [Parameter(0, "权益计算方法。0：核心权益法，1：总权益法，2：抵减总权益法，3：初始权益法，4：控制损失初始权益法，5：控制损失总权益法，6：控制损失抵减总权益法")]
        public int EquityEvaluationMethod { get; set; }

        public override string Name
        {
            get { return "波动率百分比模型"; }
        }

        public override string Description
        {
            get { return "每份头寸的波动率是总权益的固定比例(PecentageOfEquityForEachPositionVolatility / 100.0)"; }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (PercentageOfEquityForEachPositionVolatility <= 0.0 || PercentageOfEquityForEachPositionVolatility > 100.0)
            {
                throw new ArgumentOutOfRangeException("PecentageOfEquityForPositionRisk is not in (0.0, 100.0]");
            }

            _equityEvaluationMethod = (EquityEvaluationMethod)EquityEvaluationMethod;
            if (!Enum.IsDefined(typeof(EquityEvaluationMethod), _equityEvaluationMethod))
            {
                throw new ArgumentOutOfRangeException("EquityEvaluationMethod is not a valid value");
            }

            if(VolatilityWindowSize <= 0)
            {
                throw new ArgumentNullException("VolatilityWindowSize must be greater than 0");
            }
        }

        protected override Func<GenericRuntimeMetric> Creator
        {
            get { return (() => new GenericRuntimeMetric(string.Format("ATR[{0}]", VolatilityWindowSize))); }
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, out string comments)
        {
            var metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            var volatility = metric.LatestData[0][0];

            var currentEquity = Context.GetCurrentEquity(Period, _equityEvaluationMethod);

            var size = (int)(currentEquity * PercentageOfEquityForEachPositionVolatility / 100.0 / volatility);
            comments = string.Format(
                "positionsize({3}) = CurrentEquity({0:0.000}) * PercentageOfEquityForEachPositionVolatility({1:0.000}) / 100.0 / Volatility({2:0.000})",
                currentEquity,
                PercentageOfEquityForEachPositionVolatility,
                volatility,
                size);

            return size;
        }
    }
}
