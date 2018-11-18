namespace StockAnalysis.TradingStrategy.Strategy
{
    using System;
    using Base;
    using MetricBooleanExpression;

    [DeprecatedStrategy]
    public sealed class UpFromLowestFilterMarketEntering
        : MetricBasedMarketEntering
    {
        [Parameter(10, "局部最低点计算周期")]
        public int LowestCalculationPeriod { get; set; }

        [Parameter(1, "距局部最低点的最小周期")]
        public int MinPeriodAwayFromLowest { get; set; }

        [Parameter(5, "距局部最低点的最大周期")]
        public int MaxPeriodAwayFromLowest { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (LowestCalculationPeriod <= 0 || MinPeriodAwayFromLowest <= 0 || MaxPeriodAwayFromLowest <= 0)
            {
                throw new ArgumentException("No parameter could be smaller or equal to 0");
            }
        }

        protected override MetricBooleanExpression.IMetricBooleanExpression BuildExpression()
        {
            return new LogicAnd(
                new Comparison(
                    string.Format(
                        "LO[{0}].INDEX >= {1:0.000}", 
                        LowestCalculationPeriod, 
                        LowestCalculationPeriod - MaxPeriodAwayFromLowest - 1)),
                new Comparison(
                    string.Format(
                        "LO[{0}].INDEX <= {1:0.000}", 
                        LowestCalculationPeriod, 
                        LowestCalculationPeriod - MinPeriodAwayFromLowest - 1)));
        }

        public override string Name
        {
            get { return "从最低点上升入市"; }
        }

        public override string Description
        {
            get { return "当价格从局部最低点上升并且距离局部最低点的距离在范围内允许入市"; }
        }
    }
}
