using System;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class RawMetricStopLoss 
        : MetricBasedStoploss
    {
        [Parameter("ATR[10]", "原始指标")]
        public string RawMetric { get; set; }

        [Parameter(1.0, "原始指标比例尺")]
        public double RawMetricScale { get; set; }

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

        protected override double Scale
        {
            get { return RawMetricScale; }
        }
    }
}
