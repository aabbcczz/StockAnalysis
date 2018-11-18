﻿using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.TradingStrategy.Base;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class RsiMarketExiting
        : MetricBasedMarketExiting
    {
        [Parameter(2, "RSI周期")]
        public int RsiPeriod { get; set; }

        [Parameter(50.0, "RSI阈值")]
        public double Threshold { get; set; }

        [Parameter(1, "触发条件。1表示RSI高于Threshold触发，0表示RSI低于Threshold触发")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (RsiPeriod <= 0)
            {
                throw new ArgumentException("RsiPeriod value can't be smaller than 0");
            }

            if (TriggeringCondition != 0 && TriggeringCondition != 1)
            {
                throw new ArgumentException("TriggeringCondition must be 0 or 1");
            }
        }

        protected override MetricBooleanExpression.IMetricBooleanExpression BuildExpression()
        {
            return new Comparison(
                string.Format(
                    "RSI[{0}] {1} {2:0.000}",
                    RsiPeriod,
                    TriggeringCondition == 0 ? '<' : '>',
                    Threshold));
        }

        public override string Name
        {
            get { return "RSI退市"; }
        }

        public override string Description
        {
            get { return "当RSI和阈值满足触发条件时退市"; }
        }
    }
}
