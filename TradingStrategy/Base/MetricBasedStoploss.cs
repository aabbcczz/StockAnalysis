﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Base
{
    public abstract class MetricBasedStoploss : GeneralStopLossBase
    {
        private RuntimeMetricProxy _proxy;

        protected override void RegisterMetric()
        {
            base.RegisterMetric();

            _proxy = new RuntimeMetricProxy(Context.MetricManager, Metric);
        }

        protected abstract string Metric
        {
            get;
        }

        public override bool DoesStopLossDependsOnPrice
        {
            get
            {
                return false;
            }
        }

        public override double EstimateStopLossGap(ITradingObject tradingObject, double assumedPrice, out string comments)
        {
            var value = _proxy.GetMetricValues(tradingObject)[0];
            var stopLossGap = Math.Min(0.0, value - assumedPrice);

            comments = string.Format(
                "StoplossGap({3:0.000}) ~= {0}:{1:0.000} - {2:0.000}",
                Metric,
                value,
                assumedPrice,
                stopLossGap);

            return stopLossGap;
        }
    }
}