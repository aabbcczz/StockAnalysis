namespace StockAnalysis.TradingStrategy.Strategy
{
    using System;
    using Base;
    using MetricBooleanExpression;

    public sealed class CumulativeRsiMarketExiting
        : MetricBasedMarketExiting
    {
        [Parameter(2, "RSI周期")]
        public int RsiPeriod { get; set; }

        [Parameter(2, "Cumulative RSI周期")]
        public int CrsiPeriod { get; set; }

        [Parameter(90.0, "Cumulative RSI阈值")]
        public double Threshold { get; set; }

        [Parameter(1, "触发条件。1表示Cumulative RSI高于Threshold触发，0表示Cumulative RSI低于Threshold触发")]
        public int TriggeringCondition { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (RsiPeriod <= 0)
            {
                throw new ArgumentException("RsiPeriod value can't be smaller than 0");
            }

            if (CrsiPeriod <= 0)
            {
                throw new ArgumentException("CrsiPeriod value can't be smaller than 0");
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
                    "CRSI[{0},{1}] {2} {3:0.000}",
                    RsiPeriod,
                    CrsiPeriod,
                    TriggeringCondition == 0 ? '<' : '>',
                    Threshold));
        }

        public override string Name
        {
            get { return "CRSI退市"; }
        }

        public override string Description
        {
            get { return "当CRSI和阈值满足触发条件时退市"; }
        }
    }
}
