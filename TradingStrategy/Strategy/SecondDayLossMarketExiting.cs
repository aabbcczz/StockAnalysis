using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class SecondDayLossMarketExiting 
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _firstDayBarProxy;

        public override string Name
        {
            get { return "第二天亏损退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有第二天就亏损则退出市场"; }
        }

        [Parameter(0.0, "最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentage { get; set; }

        [Parameter(0.0, "第二天开盘相对第一天开盘收盘小者的最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageOpenToFirstDayMin { get; set; }

        [Parameter(0.0, "第二天收盘相对第一天开盘收盘小者的最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageCloseToFirstDayMin { get; set; }

        [Parameter(TradingPricePeriod.CurrentPeriod, "退出周期。0/CurrentPeriod为本周期，1/NextPeriod为下周期")]
        public TradingPricePeriod ExitingPeriod { get; set; }

        [Parameter(TradingPriceOption.ClosePrice, @"退出价格选项。
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
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            if(Context.ExistsPosition(tradingObject.Code))
            {
                var position = Context.GetPositionDetails(tradingObject.Code).First();
                if (position.LastedPeriodCount == 1)
                {
                    var firstDayBar = _firstDayBarProxy.GetMetricValues(tradingObject);
                    var firstDayClosePrice = firstDayBar[0];
                    var firstDayOpenPrice = firstDayBar[1];
                    var firstDayMinPrice = Math.Min(firstDayOpenPrice, firstDayClosePrice);

                    var secondDayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                    var lossPercentage = (secondDayBar.ClosePrice - secondDayBar.OpenPrice) / secondDayBar.OpenPrice * 100.0;
                    var lossPercentageOpenToFirstDayMin = (secondDayBar.OpenPrice - firstDayMinPrice) / firstDayMinPrice * 100.0;
                    var lossPercentageCloseToFirstDayMin = (secondDayBar.ClosePrice - firstDayMinPrice) / firstDayMinPrice * 100.0;

                    if (lossPercentageOpenToFirstDayMin < -MinLossPercentageOpenToFirstDayMin)
                    {
                        result.Comments = string.Format("2nd day loss: today open price {0:0.000}, first day min price {1:0.000}", secondDayBar.OpenPrice, firstDayMinPrice);

                        result.Price = new TradingPrice(TradingPricePeriod.CurrentPeriod, TradingPriceOption.OpenPrice, 0.0);

                        result.ShouldExit = true;
                    }
                    else if (lossPercentage < -MinLossPercentage)
                    {
                        result.Comments = string.Format("2nd day loss: today open price {0:0.000}, close price {1:0.000}", secondDayBar.OpenPrice, secondDayBar.ClosePrice);

                        result.Price = new TradingPrice(ExitingPeriod, ExitingPriceOption, ExitingCustomPrice);

                        result.ShouldExit = true;
                    }
                    else if (lossPercentageCloseToFirstDayMin < -MinLossPercentageCloseToFirstDayMin)
                    {
                        result.Comments = string.Format("2nd day loss: today close price {0:0.000}, first day min price {1:0.000}", secondDayBar.ClosePrice, firstDayMinPrice);

                        result.Price = new TradingPrice(ExitingPeriod, ExitingPriceOption, ExitingCustomPrice);

                        result.ShouldExit = true;
                    }
                }
            }

            return result;
        }
    }
}
