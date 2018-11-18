using System;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class VolatilityPercentagePositionSizing
        : GeneralPositionSizingBase
    {
        private RuntimeMetricProxy _atrMetricProxy;

        [Parameter(10, "波动率计算时间窗口大小")]
        public int VolatilityWindowSize { get; set; }

        [Parameter(1.0, "每份头寸的波动率占权益的百分比")]
        public double PercentageOfEquityForEachPositionVolatility { get; set; }

        [Parameter(EquityEvaluationMethod.InitialEquity, "权益计算方法。0：核心权益法，1：总权益法，2：抵减总权益法，3：初始权益法，4：控制损失初始权益法，5：控制损失总权益法，6：控制损失抵减总权益法")]
        public EquityEvaluationMethod EquityEvaluationMethod { get; set; }

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

            if(VolatilityWindowSize <= 0)
            {
                throw new ArgumentNullException("VolatilityWindowSize must be greater than 0");
            }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();
            _atrMetricProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("ATR[{0}]", VolatilityWindowSize));
        }

        public override PositionSizingComponentResult EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, int totalNumberOfObjectsToBeEstimated)
        {
            var values = _atrMetricProxy.GetMetricValues(tradingObject);

            var volatility = values[0];

            var currentEquity = Context.GetCurrentEquity(CurrentPeriod, EquityEvaluationMethod);

            var size = (int)(currentEquity * PercentageOfEquityForEachPositionVolatility / 100.0 / volatility);
            var comments = string.Format(
                "positionsize({3}) = CurrentEquity({0:0.000}) * PercentageOfEquityForEachPositionVolatility({1:0.000}) / 100.0 / Volatility({2:0.000})",
                currentEquity,
                PercentageOfEquityForEachPositionVolatility,
                volatility,
                size);

            return new PositionSizingComponentResult()
                {
                    Comments = comments,
                    PositionSize = size
                };
        }
    }
}
