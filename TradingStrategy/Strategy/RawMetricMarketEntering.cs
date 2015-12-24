using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy.Base;
using TradingStrategy.MetricBooleanExpression;

namespace TradingStrategy.Strategy
{
    public sealed class RawMetricMarketEntering
        : MetricBasedMarketEntering
    {
        [Parameter("ATR[20]", "原始指标")]
        public string RawMetric { get; set; }

        [Parameter(10.0, "阈值")]
        public double Threshold { get; set; }

        [Parameter(1, "触发条件。1表示指标值高于Threshold触发, 0表示指标值低于Threshold触发")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (string.IsNullOrWhiteSpace(RawMetric))
            {
                throw new ArgumentNullException("RawMetric");
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
                    "{0} {1} {2:0.000}",
                    RawMetric,
                    TriggeringCondition == 0 ? '<' : '>',
                    Threshold));
        }

        public override string Name
        {
            get { return "原始指标入市"; }
        }

        public override string Description
        {
            get { return "当指标值和阈值满足触发条件时允许入市"; }
        }
    }
}
