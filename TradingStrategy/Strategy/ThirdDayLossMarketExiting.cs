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


        [Parameter(0.0, "第三天开盘相对第一天开盘收盘小者的最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToFirstDayMin { get; set; }


        [Parameter(0.0, "第三天开盘相对第二天开盘收盘小者的最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToSecondDayMin { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _firstDayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[2]");

            _secondDayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if(Context.ExistsPosition(tradingObject.Symbol))
            {
                var position = Context.GetPositionDetails(tradingObject.Symbol).First();
                if (position.LastedPeriodCount == 2)
                {
                    var firstDayBar = _firstDayBarProxy.GetMetricValues(tradingObject);
                    var firstDayClosePrice = firstDayBar[0];
                    var firstDayOpenPrice = firstDayBar[1];
                    var firstDayMinPrice = Math.Min(firstDayClosePrice, firstDayOpenPrice);

                    var secondDayBar = _secondDayBarProxy.GetMetricValues(tradingObject);
                    var secondDayClosePrice = secondDayBar[0];
                    var secondDayOpenPrice = secondDayBar[1];
                    var secondDayMinPrice = Math.Min(secondDayOpenPrice, secondDayClosePrice);

                    var thirdDayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                    var lossPercentageOpenToFirstDayMin = (thirdDayBar.OpenPrice - firstDayMinPrice) / firstDayMinPrice * 100.0;
                    var lossPercentageOpenToSecondDayMin = (thirdDayBar.OpenPrice - secondDayMinPrice) / secondDayMinPrice * 100.0;


                    if (lossPercentageOpenToFirstDayMin < -MinLossPercentageOpenToFirstDayMin
                        || lossPercentageOpenToSecondDayMin < -MinLossPercentageOpenToSecondDayMin)
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
