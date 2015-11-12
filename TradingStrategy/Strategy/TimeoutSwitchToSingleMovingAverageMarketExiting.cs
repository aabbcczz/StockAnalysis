using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class TimeoutSwitchToSingleMovingAverageMarketExiting 
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _highestMetricProxy;
        private RuntimeMetricProxy _movingAverageMetricProxy;

        private HashSet<string> _codesSwitchedToSingleMovingAverageMarketExiting = new HashSet<string>();

 
        public override string Name
        {
            get { return "定时转单移动平均退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有在一段时间内若创新高则转为按单移动平均退出，否则立即退出市场"; }
        }

        [Parameter(2, "头寸持有周期数")]
        public int HoldingPeriods { get; set; }

        [Parameter(20, "移动平均周期数")]
        public int MovingAveragePeriods { get; set; }

        [Parameter(20, "判定新高周期数")]
        public int HighestLookbackPeriods { get; set; }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (HoldingPeriods < 0)
            {
                throw new ArgumentOutOfRangeException("HoldingPeriods must be greater than 0");
            }

            if (MovingAveragePeriods <= 0)
            {
                throw new ArgumentOutOfRangeException("MovingAveragePeriods must be greater than 0");
            }

            if (HighestLookbackPeriods <= 0)
            {
                throw new ArgumentOutOfRangeException("HighestJudgementPeriods must be greater than 0");
            }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _highestMetricProxy = new RuntimeMetricProxy(
                Context.MetricManager, 
                string.Format("HI[{0}](BAR.HP)", HighestLookbackPeriods));

            _movingAverageMetricProxy = new RuntimeMetricProxy(
                Context.MetricManager,
                string.Format("MA[{0}]", MovingAveragePeriods));
        }

        public override MarketExitingComponentResult ShouldExit(ITradingObject tradingObject)
        {
            var result = new MarketExitingComponentResult();

            var code = tradingObject.Code;
            if (Context.ExistsPosition(code))
            {
                Bar todayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);

                if (_codesSwitchedToSingleMovingAverageMarketExiting.Contains(code))
                {
                    double movingAverage = _movingAverageMetricProxy.GetMetricValues(tradingObject)[0];

                    if (todayBar.ClosePrice < movingAverage)
                    {
                        result.ShouldExit = true;
                        result.Comments = string.Format(
                            "Close price {0:0.000} < MA[{1}]({2:0.000})",
                            todayBar.ClosePrice,
                            MovingAveragePeriods,
                            movingAverage);

                        result.Price = new TradingPrice(
                            TradingPricePeriod.CurrentPeriod,
                            TradingPriceOption.ClosePrice,
                            0.0);
                    }
                }
                else
                {
                    int periodCount = Context.GetPositionDetails(code).Last().LastedPeriodCount;

                    if (periodCount >= HoldingPeriods)
                    {
                        var highestIndex = _highestMetricProxy.GetMetricValues(tradingObject)[1];

                        if (periodCount == HoldingPeriods
                            && (int)highestIndex == HighestLookbackPeriods - 1 
                            && todayBar.ClosePrice >= todayBar.OpenPrice)
                        {
                            // today is the highest price, switch to moving average exiting.
                            _codesSwitchedToSingleMovingAverageMarketExiting.Add(code);
                        }
                        else
                        {
                            result.Comments = string.Format("hold for {0} periods", HoldingPeriods);
                            result.ShouldExit = true;
                        }
                    }
                }
            }
            else
            {
                _codesSwitchedToSingleMovingAverageMarketExiting.Remove(code);
            }

            return result;
        }
    }
}
