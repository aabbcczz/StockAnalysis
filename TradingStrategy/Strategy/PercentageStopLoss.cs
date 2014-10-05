using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
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

        public override double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments)
        {
            comments = string.Format(
                "stoplossgap = price({0:0.000}) * MaxPercentageOfLoss({1:0.000}) / 100.0",
                assumedPrice,
                MaxPercentageOfLoss);

            return -(assumedPrice * MaxPercentageOfLoss / 100.0);
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
