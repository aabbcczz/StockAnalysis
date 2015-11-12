using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class ContinueTwoDayLossMarketExiting 
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _yesterdayBarProxy;

        public override string Name
        {
            get { return "连续两天亏损退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有连续两天亏损则退出市场"; }
        }

        [Parameter(0.0, "最小亏损百分比，当亏损大于此值时退出")]
        public double MinLossPercentage { get; set; }

        [Parameter(TradingPricePeriod.NextPeriod, "退出周期。0/CurrentPeriod为本周期，1/NextPeriod为下周期")]
        public TradingPricePeriod ExitingPeriod { get; set; }

        [Parameter(TradingPriceOption.OpenPrice, @"退出价格选项。
                    OpenPrice = 0,
                    ClosePrice = 1,
                    CustomPrice = 2")]
        public TradingPriceOption ExitingPriceOption { get; set; }

        [Parameter(0.0, "退出价格，当ExitingPriceOption = 2/CustomPrice时有效")]
        public double ExitingCustomPrice { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _yesterdayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if(Context.ExistsPosition(tradingObject.Code))
            {
                var position = Context.GetPositionDetails(tradingObject.Code).First();
                if (position.LastedPeriodCount == 1)
                {
                    var yesterdayBar = _yesterdayBarProxy.GetMetricValues(tradingObject);
                    var yesterDayClosePrice = yesterdayBar[0];
                    var yesterDayOpenPrice = yesterdayBar[1];

                    var todayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

                    if (todayBar.ClosePrice < todayBar.OpenPrice && yesterDayClosePrice < yesterDayOpenPrice)
                    {
                        var lossPercentage = (todayBar.ClosePrice - yesterDayOpenPrice) / yesterDayOpenPrice * 100.0;

                        if (lossPercentage < -MinLossPercentage)
                        {
                            result.Comments = string.Format("Continue 2 days loss: today close price {0:0.000}, yesterday open price {1:0.000}", todayBar.ClosePrice, yesterDayOpenPrice);

                            result.Price = new TradingPrice(ExitingPeriod, ExitingPriceOption, ExitingCustomPrice);

                            result.ShouldExit = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}
