using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class GapDownBounceOverMarketEntering
        : GeneralMarketEnteringBase
    {
        private RuntimeMetricProxy _movingAverage;
        private RuntimeMetricProxy _previousDayBar;

        [Parameter(5, "移动平均趋势线周期, 0表示忽略")]
        public int MovingAveragePeriod { get; set; }

        [Parameter(4.0, "昨日收盘价应当比移动平均低的最小百分比例")]
        public double MinPercentageBelowMovingAverage { get; set; }

        [Parameter(1.0, "跳空开盘价低于昨日最低价的最小百分比")]
        public double MinPercentageOfGapDown { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _movingAverage = MovingAveragePeriod > 0 
                ? new RuntimeMetricProxy(
                    Context.MetricManager,
                    string.Format("REF[1](MA[{0}])", MovingAveragePeriod))
                : null;

            _previousDayBar = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");
        }

        public override string Name
        {
            get { return "跳空后盘中反弹过昨日最低点入市策略"; }
        }

        public override string Description
        {
            get { return "当昨日收盘价低于移动平均一定程度并且今日开盘跳空后盘中昨日收盘后以昨日收盘价入市"; }
        }

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            var previousDayBarValues = _previousDayBar.GetMetricValues(tradingObject);

            var todayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
            double movingAverage = _movingAverage == null ? 1000000.00 : _movingAverage.GetMetricValues(tradingObject)[0];
            var previousDayBarLowest = previousDayBarValues[3];
            var previousDayBarClose = previousDayBarValues[0];

            if (previousDayBarClose < movingAverage * (100.0 - MinPercentageBelowMovingAverage) / 100.0 // below average
                && todayBar.OpenPrice < previousDayBarLowest * (100.0 - MinPercentageOfGapDown) / 100.0 // gap down
                && todayBar.HighestPrice > previousDayBarLowest // bounce over last day lowest
                )
            { 
                result.Comments = string.Format(
                    "MA[{0}]={1:0.000} Highest:{2:0.000} Open:{3:0.000} LastLowest:{4:0.000}", 
                    MovingAveragePeriod,
                    movingAverage,
                    todayBar.HighestPrice,
                    todayBar.OpenPrice,
                    previousDayBarLowest);

                result.CanEnter = true;

                result.Price = new TradingPrice(TradingPricePeriod.CurrentPeriod, TradingPriceOption.CustomPrice, previousDayBarLowest);
            }

            return result;
        }
    }
}
