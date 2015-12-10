using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class ThirdDayLossMarketExiting 
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _firstDayBarProxy;
        private RuntimeMetricProxy _secondDayBarProxy;

        public override string Name
        {
            get { return "第三天开盘条件退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有第三天开盘符合条件就退出市场"; }
        }


        [Parameter(0.0, "第三天开盘相对第一天收盘最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToFirstDayClose { get; set; }

        [Parameter(0.0, "第三天开盘相对第一天开盘最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToFirstDayOpen { get; set; }

        [Parameter(0.0, "第三天开盘相对第二天收盘最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToSecondDayClose { get; set; }

        [Parameter(0.0, "第三天开盘相对第二天开盘最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToSecondDayOpen { get; set; }

        [Parameter(TradingPricePeriod.NextPeriod, "退出周期。0/CurrentPeriod为本周期，1/NextPeriod为下周期")]
        public TradingPricePeriod ExitingPeriod { get; set; }

        [Parameter(TradingPriceOption.OpenPrice, @"退出价格选项。
                    OpenPrice = 0,
                    ClosePrice = 1,
                    CustomPrice = 2")]
        public TradingPriceOption ExitingPriceOption { get; set; }

        [Parameter(0.0, "退出价格, 当ExitingPriceOption = 2/CustomPrice时有效")]
        public double ExitingCustomPrice { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _firstDayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");

            _secondDayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[2]");
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if(Context.ExistsPosition(tradingObject.Code))
            {
                var position = Context.GetPositionDetails(tradingObject.Code).First();
                if (position.LastedPeriodCount == 2)
                {
                    var firstDayBar = _firstDayBarProxy.GetMetricValues(tradingObject);
                    var firstDayClosePrice = firstDayBar[0];
                    var firstDayOpenPrice = firstDayBar[1];

                    var secondDayBar = _secondDayBarProxy.GetMetricValues(tradingObject);
                    var secondDayClosePrice = secondDayBar[0];
                    var secondDayOpenPrice = secondDayBar[1];

                    var thirdDayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                    var lossPercentageOpenToFirstDayClose = (thirdDayBar.OpenPrice - firstDayClosePrice) / firstDayClosePrice * 100.0;
                    var lossPercentageOpenToFirstDayOpen = (thirdDayBar.OpenPrice - firstDayOpenPrice) / firstDayOpenPrice * 100.0;
                    var lossPercentageOpenToSecondDayClose = (thirdDayBar.OpenPrice - secondDayClosePrice) / secondDayClosePrice * 100.0;
                    var lossPercentageOpenToSecondDayOpen = (thirdDayBar.OpenPrice - secondDayOpenPrice) / secondDayOpenPrice * 100.0;


                    if (lossPercentageOpenToFirstDayClose < -MinLossPercentageOpenToFirstDayClose
                        || lossPercentageOpenToFirstDayOpen < -MinLossPercentageOpenToFirstDayOpen
                        || lossPercentageOpenToSecondDayClose < -MinLossPercentageOpenToSecondDayClose
                        || lossPercentageOpenToSecondDayOpen < -MinLossPercentageOpenToSecondDayOpen)
                    {
                        result.Comments = string.Format(
                            "3rd day loss: 3rd day open price {0:0.000}, 1st day close price {1:0.000}, 1st day open price {2:0.000}, 2nd day close price {1:0.000}, 2nd day open price {2:0.000}", 
                            thirdDayBar.OpenPrice, 
                            firstDayClosePrice,
                            firstDayOpenPrice,
                            secondDayClosePrice,
                            secondDayOpenPrice);

                        result.Price = new TradingPrice(TradingPricePeriod.CurrentPeriod, TradingPriceOption.OpenPrice, 0.0);

                        result.ShouldExit = true;
                    }
                }
            }

            return result;
        }
    }
}
