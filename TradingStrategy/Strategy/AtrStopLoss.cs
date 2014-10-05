﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public sealed class AtrStopLoss 
        : MetricBasedStopLossBase<AtrRuntimeMetric>
    {
        [Parameter(10, "ATR计算窗口大小")]
        public int AtrWindowSize { get; set; }

        [Parameter(3.0, "ATR停价倍数")]
        public double AtrStopLossFactor { get; set; }


        public override string Name
        {
            get { return "ATR停价"; }
        }

        public override string Description
        {
            get { return "当价格低于买入价，并且差值大于ATR乘以Atr停价倍数时停价"; }
        }

        public override Func<AtrRuntimeMetric> Creator
        {
            get 
            {
                return (() => { return new AtrRuntimeMetric(AtrWindowSize); });
            }
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (AtrWindowSize <= 1 || AtrStopLossFactor <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public override double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments)
        {
            AtrRuntimeMetric metric = MetricManager.GetOrCreateRuntimeMetric(tradingObject);

            comments = string.Format(
                "stoplossgap = ATR({0:0.000}) * AtrStopLossFactor({1:0.000})",
                metric.Atr,
                AtrStopLossFactor);

            return -metric.Atr * AtrStopLossFactor;
        }
    }
}
