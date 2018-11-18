namespace StockAnalysis.TradingStrategy.Strategy
{
    using Base;

    public sealed class RawMetricTraceStopLossMarketExiting
        : MetricBasedTraceStopLossMarketExiting
    {
        [Parameter("ATR[10]", "原始指标")]
        public string RawMetric { get; set; }

        public override string Name
        {
            get { return "原始指标跟踪停价"; }
        }

        public override string Description
        {
            get { return "以原始指标的值跟踪停价"; }
        }

        protected override string Metric
        {
            get { return RawMetric; }
        }
    }
}
