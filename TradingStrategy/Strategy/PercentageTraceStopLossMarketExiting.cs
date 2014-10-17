using System;

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

        protected override double CalculateStopLossPrice(ITradingObject tradingObject, double currentPrice)
        {
            return currentPrice * (1 - MaxPercentageOfDrawDown / 100.0);
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
