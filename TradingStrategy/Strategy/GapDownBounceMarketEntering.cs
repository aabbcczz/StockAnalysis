using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class GapDownBounceMarketEntering
        : GeneralMarketEnteringBase
    {
        private const int LowestPeriod = 5;

        private RuntimeMetricProxy _movingAverage;
        private RuntimeMetricProxy _previousDayBar;
        private RuntimeMetricProxy _previousTwoDaysBar;

        [Parameter(5, "移动平均趋势线周期, 0表示忽略")]
        public int MovingAveragePeriod { get; set; }

        [Parameter(4.0, "收盘价应当比移动平均低的最小百分比例")]
        public double MinPercentageBelowMovingAverage { get; set; }

        [Parameter(1.0, "跳空开盘价低于昨日最低价的最小百分比")]
        public double MinPercentageOfGapDown { get; set; }

        [Parameter(0.0, "反弹超过昨日最低价的最小百分比")]
        public double MinBouncePercentageOverLastLowestPrice { get; set; }

        [Parameter(50.0, "上影线所允许的最大比例")]
        public double MaxUpShadowPercentage { get; set; }

        [Parameter(50.0, "下影线所允许的最大比例")]
        public double MaxDownShadowPercentage { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            if (MovingAveragePeriod > 0)
            {
                _movingAverage = new RuntimeMetricProxy(
                    Context.MetricManager,
                    string.Format("MA[{0}]", MovingAveragePeriod));
            }
            else
            {
                _movingAverage = null;
            }

            _previousDayBar = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");

            _previousTwoDaysBar = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[2]");
        }

        public override string Name
        {
            get { return "跳空后反弹入市策略"; }
        }

        public override string Description
        {
            get { return "当收盘价低于移动平均一定程度，并且开盘跳空，收盘超过昨日收盘后入市"; }
        }

        private bool IsDescending(double[] bar, double[] previousBar)
        {
            // C, O, H, L

            // ensure the bar is descending
            //if (bar[0] > bar[1])
            //{
            //    return false;
            //}

            // ensure the previous bar is descending
            //if (previousBar[0] > previousBar[1])
            //{
            //    return false;
            //}

            // ensure the lowest value of previous bar is higher than the lowest of current bar.
            //if (bar[3] > previousBar[3])
            //{
            //    return false;
            //}

            // ensure the close of current bar is lower than the mininum value of open and close of previous bar
            //if (Math.Min(bar[0], bar[1]) > Math.Min(previousBar[0], previousBar[1]))
            //{
            //    return false;
            //}

            return true;
        }

        public override MarketEnteringComponentResult CanEnter(ITradingObject tradingObject)
        {
            var result = new MarketEnteringComponentResult();

            var previousDayBarValues = _previousDayBar.GetMetricValues(tradingObject);
            var previousTwoDaysBarValues = _previousTwoDaysBar.GetMetricValues(tradingObject);

            if (!IsDescending(previousDayBarValues, previousTwoDaysBarValues))
            {
                return result;
            }

            var todayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
            double movingAverage = _movingAverage == null ? 1000000.00 : _movingAverage.GetMetricValues(tradingObject)[0];
            var previousDayBarLowest = previousDayBarValues[3];

            var upShadowPercentage = Math.Abs(todayBar.HighestPrice - todayBar.LowestPrice) < 1e-6
                ? 100.0
                : (todayBar.HighestPrice - todayBar.ClosePrice) / (todayBar.HighestPrice - todayBar.LowestPrice) * 100.0;

            var downShadowPercentage = Math.Abs(todayBar.HighestPrice - todayBar.LowestPrice) < 1e-6
                ? 0.0
                : (todayBar.OpenPrice - todayBar.LowestPrice) / (todayBar.HighestPrice - todayBar.LowestPrice) * 100.0;

            if (todayBar.ClosePrice < movingAverage * (100.0 - MinPercentageBelowMovingAverage) / 100.0 // below average
                && todayBar.OpenPrice < previousDayBarLowest * (100.0 - MinPercentageOfGapDown) / 100.0 // gap down
                && todayBar.ClosePrice > previousDayBarLowest * (100.0 + MinBouncePercentageOverLastLowestPrice) / 100.0 // bounce over last day
                && upShadowPercentage <= MaxUpShadowPercentage
                && downShadowPercentage <= MaxDownShadowPercentage
                )
            { 
                result.Comments = string.Format(
                    "MA[{0}]={1:0.000} Close:{2:0.000} Open:{3:0.000} LastLowest:{4:0.000} UpShadow%:{5:0.000}% DownShadow%:{6:0.000}%", 
                    MovingAveragePeriod,
                    movingAverage,
                    todayBar.ClosePrice,
                    todayBar.OpenPrice,
                    previousDayBarLowest,
                    upShadowPercentage,
                    downShadowPercentage);

                result.CanEnter = true;
            }

            return result;
        }
    }
}
