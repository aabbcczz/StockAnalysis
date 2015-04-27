using System;
using TradingStrategy.Base;

namespace TradingStrategy.Strategy
{
    public sealed class RawMetricStopLoss 
        : MetricBasedStoploss
    {
        [Parameter("ATR[10]", "原始指标")]
        public string RawMetric { get; set; }

        public override string Name
        {
            get { return "原始指标停价"; }
        }

        public override string Description
        {
            get { return "以原始指标的值作为停价"; }
        }

        protected override string Metric
        {
            get { return RawMetric; }
        }
    }
}
