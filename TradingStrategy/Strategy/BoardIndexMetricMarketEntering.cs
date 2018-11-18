using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.TradingStrategy.Base;
using StockAnalysis.TradingStrategy.MetricBooleanExpression;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class BoardIndexMetricMarketEntering
        : BoardIndexMetricBasedMarketEntering
    {
        [Parameter("RSI[20]", "指标定义")]
        public string Metric { get; set; }

        [Parameter(10.0, "阈值")]
        public double Threshold { get; set; }

        [Parameter(1, "触发条件。1表示指标值>阈值触发, 0指标值<阈值触发")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (string.IsNullOrWhiteSpace(Metric))
            {
                throw new ArgumentException("No metric is specified");
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
                    "{0} {1} {2}",
                    Metric,
                    TriggeringCondition == 0 ? '<' : '>',
                    Threshold));
        }

        public override string Name
        {
            get { return "板块指数指标入市"; }
        }

        public override string Description
        {
            get { return "当板块指数指标和给定阈值满足触发条件时允许入市"; }
        }
    }
}
