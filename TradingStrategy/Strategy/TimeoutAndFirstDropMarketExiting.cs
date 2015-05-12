using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class TimeoutAndFirstDropMarketExiting 
        : GeneralMarketExitingBase
    {
        private readonly PeriodCounter<DateTime> _periodCounter = new PeriodCounter<DateTime>();
        private RuntimeMetricProxy _referenceBar;

        public override string Name
        {
            get { return "定时后出现首次下降退出"; }
        }

        public override string Description
        {
            get { return "当头寸持有超过一段时间后，并且开盘价未跳空高开，或者收阴线，或者收盘价出现首次下降立即退出市场"; }
        }

        [Parameter(20, "头寸持有周期数")]
        public int HoldingPeriods { get; set; }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _referenceBar = new RuntimeMetricProxy(
                Context.MetricManager,
                "REFBAR[1]");
        }

        protected override void ValidateParameterValues()
        {
 	        base.ValidateParameterValues();

            if (HoldingPeriods < 0)
            {
                throw new ArgumentOutOfRangeException("HoldingPeriods must be great than 0");
            }
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            var code = tradingObject.Code;
            if (Context.ExistsPosition(code))
            {
                var latestBuyTime = Context.GetPositionDetails(code).Max(p => p.BuyTime);

                if (!_periodCounter.Exists(tradingObject))
                {
                    var periodCount = latestBuyTime < CurrentPeriod ? 1 : 0;

                    _periodCounter.InitializeOrUpdate(tradingObject, periodCount, latestBuyTime);
                }
                else
                {
                    DateTime prevPositionLatestBuyTime;
                    _periodCounter.GetPeriod(tradingObject, out prevPositionLatestBuyTime);

                    if (latestBuyTime > prevPositionLatestBuyTime)
                    {
                        // new postion has been created, we need to reset record
                        var periodCount = latestBuyTime < CurrentPeriod ? 1 : 0;

                        _periodCounter.Remove(tradingObject);
                        _periodCounter.InitializeOrUpdate(tradingObject, periodCount, latestBuyTime);

                    }
                    else
                    {
                        // just update period
                        _periodCounter.InitializeOrUpdate(tradingObject, 0);
                    }
                }
            }
            else
            {
                _periodCounter.Remove(tradingObject);
            }
        }

        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            int periodCount = _periodCounter.GetPeriod(tradingObject);

            if (periodCount >= HoldingPeriods)
            {
                var todayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                var previousBar = _referenceBar.GetMetricValues(tradingObject);
                var previousOpen = previousBar[1];
                var previousClose = previousBar[0];

                if (todayBar.OpenPrice  < previousClose
                    || todayBar.ClosePrice < previousClose
                    || todayBar.ClosePrice < todayBar.OpenPrice)
                {
                    comments = string.Format(
                        "hold for {0} periods and no jump up and rise. today open {1:0.000}, today close {2:0.000} previous close {3:0.000}", 
                        HoldingPeriods,
                        todayBar.OpenPrice,
                        todayBar.ClosePrice,
                        previousClose);

                    return true;
                }
            }

            return false;
        }
    }
}
