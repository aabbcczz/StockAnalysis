using System;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class PercentageStopLoss 
        : GeneralStopLossBase
    {
        [Parameter(5.0, "最大损失的百分比")]
        public double MaxPercentageOfLoss { get; set; }

        public override string Name
        {
            get { return "百分比折回停价"; }
        }

        public override string Description
        {
            get { return "如果当前价格低于买入价的一定百分比则触发停价"; }
        }

        public override StopLossComponentResult EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice)
        {
            var stoplossGap = -(assumedPrice * MaxPercentageOfLoss / 100.0);
            var comments = string.Format(
                "stoplossgap({2:0.000}) = price({0:0.000}) * MaxPercentageOfLoss({1:0.000}) / 100.0",
                assumedPrice,
                MaxPercentageOfLoss,
                stoplossGap);

            return new StopLossComponentResult()
                {
                    Comments = comments,
                    StopLossGap = stoplossGap
                };
        }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (MaxPercentageOfLoss <= 0.0 || MaxPercentageOfLoss > 100.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
