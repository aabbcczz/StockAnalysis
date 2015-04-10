﻿using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class SingleMovingAverageMarketExiting
        : MetricBasedMarketExiting
    {
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

        protected override IMetricBooleanExpression BuildExpression()
        {
            return new Comparison(
                string.Format(
                    "BAR.CP {0} MA[{1}]",
                    TriggeringCondition == 0 ? '<' : '>',
                    MovingAveragePeriod));
        }

        public override string Name
        {
            get { return "单移动平均退市"; }
        }

        public override string Description
        {
            get { return "当收盘价和移动平均值满足触发条件时退市"; }
        }
    }
}