using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class SingleMovingAverageFilterMarketExiting
        : GeneralMarketExitingBase
    {
        private RuntimeMetricProxy _movingAverageProxy;

        [Parameter(10, "移动平均周期")]
        public int MovingAveragePeriod { get; set; }

        [Parameter(0, "触发条件。1表示收盘价高于移动平均值触发，0表示收盘价低于移动平均值触发")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (MovingAveragePeriod <= 0)
            {
                throw new ArgumentException("Period value can't be smaller than 0");
            }

            if (TriggeringCondition != 0 && TriggeringCondition != 1)
            {
                throw new ArgumentException("TriggeringCondition must be 0 or 1");
            }
        }

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _movingAverageProxy = new RuntimeMetricProxy(Context.MetricManager, string.Format("MA[{0}]", MovingAveragePeriod));
        }

        public override string Name
        {
            get { return "单移动平均退市过滤器"; }
        }

        public override string Description
        {
            get { return "当收盘价和移动平均值满足触发条件时退市"; }
        }


        public override bool ShouldExit(ITradingObject tradingObject, out string comments)
        {
            comments = string.Empty;

            var closePrice = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject).ClosePrice;
            var movingAverage = _movingAverageProxy.GetMetricValues(tradingObject)[0];

            if ((TriggeringCondition == 0 && closePrice < movingAverage)
                || (TriggeringCondition == 1 && closePrice > movingAverage))
            {
                comments = string.Format(
                        "CP:{0:0.000} {1} MA[{2}]:{3:0.000} ",
                        closePrice,
                        TriggeringCondition == 0 ? '<' : '>',
                        MovingAveragePeriod,
                        movingAverage);

                return true;
            }

            return false;
        }
    }
}
