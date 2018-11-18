namespace StockAnalysis.TradingStrategy.Strategy
{
    using System;
    using System.Linq;
    using Base;
    public sealed class TimeoutMarketExiting 
        : GeneralMarketExitingBase
    {
        public override string Name
        {
            get { return "定时退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有超过一段时间后立即退出市场"; }
        }

        [Parameter(20, "头寸持有周期数")]
        public int HoldingPeriods { get; set; }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (HoldingPeriods < 0)
            {
                throw new ArgumentOutOfRangeException("HoldingPeriods must be great than 0");
            }
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            var symbol = tradingObject.Symbol;
            if (Context.ExistsPosition(symbol))
            {
                int periodCount = Context.GetPositionDetails(symbol).Last().LastedPeriodCount;

                if (periodCount >= HoldingPeriods)
                {
                    result.Comments = string.Format("hold for {0} periods", HoldingPeriods);
                    result.ShouldExit = true;

                    if (Context.GetPositionFrozenDays() > periodCount)
                    {
                        result.Price = new TradingPrice(TradingPricePeriod.NextPeriod, TradingPriceOption.OpenPrice, 0.0);
                    }
                }
            }

            return result;
        }
    }
}
