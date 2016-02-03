using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class FirstDayLossMarketExiting 
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _firstDayBarProxy;
        private RuntimeMetricProxy _theDayBeforeFirstDayBarProxy;

        public override string Name
        {
            get { return "第一天亏损退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有第一天就亏损则退出市场"; }
        }

        [Parameter(0.0, "最小亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentage { get; set; }

        [Parameter(0.0, "相对前一日收盘亏损百分比, 当亏损大于此值时退出")]
        public double MinLossPercentageToPreviousDayClose { get; set; }

        [Parameter(TradingPricePeriod.NextPeriod, "退出周期。0/CurrentPeriod为本周期, 1/NextPeriod为下周期")]
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

            _firstDayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");
            
            _theDayBeforeFirstDayBarProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[2]");
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
                    var theDayBeforeFirstDayBar = _theDayBeforeFirstDayBarProxy.GetMetricValues(tradingObject);

                    var firstDayClosePrice = firstDayBar[0];
                    var theDayBeforeFirstDayClosePrice = theDayBeforeFirstDayBar[0];

                    var lossPercentage = (firstDayClosePrice - position.BuyPrice) / position.BuyPrice * 100.0;
                    var lossPercentageToPreviousDay = (firstDayClosePrice - theDayBeforeFirstDayClosePrice) / theDayBeforeFirstDayClosePrice * 100.0;

                    if (lossPercentage < -MinLossPercentage 
                        || lossPercentageToPreviousDay < -MinLossPercentageToPreviousDayClose)
                    {
                        result.Comments = string.Format("Loss: buy price {0:0.000}, close price {1:0.000}, prev close price {2:0.000}", position.BuyPrice, firstDayClosePrice, theDayBeforeFirstDayClosePrice);

                        result.Price = new TradingPrice(ExitingPeriod, ExitingPriceOption, ExitingCustomPrice);

                        result.ShouldExit = true;
                    }
                }
            }

            return result;
        }
    }
}
