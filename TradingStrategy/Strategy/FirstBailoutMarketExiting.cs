namespace TradingStrategy.Strategy
{
    using System;
    using System.Linq;
    using Base;

    public sealed class FirstBailoutMarketExiting 
        : GeneralMarketExitingBase
    {
        public override string Name
        {
            get { return "首次获利退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有首次获利后退出市场"; }
        }

        [Parameter(0, "价格选择选项。0为最高价，1为最低价，2为收盘价，3为开盘价")]
        public int PriceSelector { get; set; }

        [Parameter(0, "最小保持周期数")]
        public int MinKeepPeriods { get; set; }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (!BarPriceSelector.IsValidSelector(PriceSelector))
            {
                throw new ArgumentException("价格选择项非法");
            }

            if (MinKeepPeriods < 0)
            {
                throw new ArgumentException("获利后保持周期数非法");
            }
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if (Context.ExistsPosition(tradingObject.Symbol))
            {
                var position = Context.GetPositionDetails(tradingObject.Symbol).First();

                if (position.LastedPeriodCount >= MinKeepPeriods)
                {
                    var bar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                    var price = BarPriceSelector.Select(bar, PriceSelector);

                    if (position.BuyPrice < price)
                    {
                        result.Comments = string.Format("Bailout: buy price {0:0.000}, current price {1:0.000}", position.BuyPrice, price);

                        result.ShouldExit = true;
                    }
                }
            }

            return result;
        }
    }
}
