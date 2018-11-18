using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.TradingStrategy.Base;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class RawMetricMarketEntering
        : MetricBasedMarketEntering
    {
        [Parameter("ATR[20]", "原始指标")]
        public string RawMetric { get; set; }

        [Parameter(10.0, "上阈值")]
        public double HighThreshold { get; set; }

        [Parameter(0.0, "下阈值")]
        public double LowThreshold { get; set; }

        [Parameter(1, "触发条件。1表示指标值在上下阈值之间(含)触发, 0表示指标值在上下阈值之外触发")]
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
            IMetricBooleanExpression expression = 
                new LogicAnd(
                    new Comparison(
                        string.Format(
                            "{0} >= {1:0.0000}",
                            RawMetric,
                            LowThreshold)),
                    new Comparison(
                        string.Format(
                            "{0} <= {1:0.0000}",
                            RawMetric,
                            HighThreshold)));
            
            if (TriggeringCondition == 0)
            {
                expression = new LogicNot(expression);
            }

            return expression;
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
