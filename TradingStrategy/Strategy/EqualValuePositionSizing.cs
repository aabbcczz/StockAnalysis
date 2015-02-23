using System;
using System.Collections.Generic;

namespace TradingStrategy.Strategy
{
    public sealed class EqualValuePositionSizing : GeneralPositionSizingBase
    {
        [Parameter(10, "权益被分割的块数，每份头寸将占有一份")]
        public int PartsOfEquity { get; set; }

        [Parameter(EquityEvaluationMethod.InitialEquity, "权益计算方法。0：核心权益法，1：总权益法，2：抵减总权益法，3：初始权益法，4：控制损失初始权益法，5：控制损失总权益法，6：控制损失抵减总权益法")]
        public EquityEvaluationMethod EquityEvaluationMethod { get; set; }

        public override string Name
        {
            get { return "价格等值模型"; }
        }

        public override string Description
        {
            get { return "每份头寸占有的价值是总权益的固定比例(1/PartsOfEquity)"; }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (PartsOfEquity < 0)
            {
                throw new ArgumentOutOfRangeException("PartsOfEquity must be greater than 0");
            }
        }

        public override int EstimatePositionSize(ITradingObject tradingObject, double price, double stopLossGap, out string comments)
        {
            var currentEquity = Context.GetCurrentEquity(CurrentPeriod, EquityEvaluationMethod);

            comments = string.Format(
                "positionsize = currentEquity({0:0.000}) / PartsOfEquity ({1}) / price({2:0.000})",
                currentEquity,
                PartsOfEquity,
                price);

            return (int)(currentEquity / PartsOfEquity / price);
        }
    }
}
