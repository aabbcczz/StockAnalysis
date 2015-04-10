using System;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class RiskPercentagePositionSizing : GeneralPositionSizingBase
    {

        [Parameter(1.0, "每份头寸的风险占权益的百分比")]
        public double PercentageOfEquityForEachRisk { get; set; }

        [Parameter(EquityEvaluationMethod.InitialEquity, "权益计算方法。0：核心权益法，1：总权益法，2：抵减总权益法，3：初始权益法，4：控制损失初始权益法，5：控制损失总权益法，6：控制损失抵减总权益法")]
        public EquityEvaluationMethod EquityEvaluationMethod { get; set; }

        public override string Name
        {
            get { return "风险百分比模型"; }
        }

        public override string Description
        {
            get { return "每份头寸的风险是总权益的固定比例(PecentageOfEquityForEachRisk / 100.0)"; }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (PercentageOfEquityForEachRisk <= 0.0 || PercentageOfEquityForEachRisk > 100.0)
            {
                throw new ArgumentOutOfRangeException("PecentageOfEquityForPositionRisk is not in (0.0, 100.0]");
            }
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, out string comments, int totalNumberOfObjectsToBeEstimated)
        {
            var currentEquity = Context.GetCurrentEquity(CurrentPeriod, EquityEvaluationMethod);

            var size = (int)(currentEquity * PercentageOfEquityForEachRisk / 100.0 / Math.Abs(stopLossGap));

            comments = string.Format(
                "positionsize({3}) = CurrentEquity({0:0.000}) * PercentageOfEquityForEachRisk({1:0.000}) / 100.0 / Risk({2:0.000})",
                currentEquity,
                PercentageOfEquityForEachRisk,
                Math.Abs(stopLossGap),
                size);

            return size;
        }
    }
}
