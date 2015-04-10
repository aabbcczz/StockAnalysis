using System;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class PercentageTraceStopLossMarketExiting 
        : GeneralTraceStopLossMarketExitingBase
    {
        [Parameter(5.0, "从最高点回撤的最大百分比")]
        public double MaxPercentageOfDrawDown { get; set; }

        public override string Name
        {
            get { return "百分比折回跟踪停价退市"; }
        }

        public override string Description
        {
            get { return "如果当前价格低于最高价的一定百分比则触发停价退市"; }
        }

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice, out string comments)
        {
            var stoploss = currentPrice * (1 - MaxPercentageOfDrawDown / 100.0);
            comments = string.Format(
                "stoploss({0:0.000}) = Price({1:0.000}) * (1 - MaxPercentageOfDrawDown({2:0.000}) / 100.0)",
                stoploss,
                currentPrice,
                MaxPercentageOfDrawDown);

            return stoploss;
        }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (MaxPercentageOfDrawDown <= 0.0 || MaxPercentageOfDrawDown > 100.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
