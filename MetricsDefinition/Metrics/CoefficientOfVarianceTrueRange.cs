namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;
    using Common.Data;

    /// <summary>
    /// The Coefficient of Variance of TureRange metric
    /// </summary>
    [Metric("COVTR")]
    public sealed class CoefficientOfVarianceTrueRange : SingleOutputBarInputSerialMetric
    {
        private readonly StdDevTrueRange _sdtr;
        private readonly AverageTrueRange _atr;
        public CoefficientOfVarianceTrueRange(int windowSize)
            : base(0)
        {
            _sdtr = new StdDevTrueRange(windowSize);
            _atr = new AverageTrueRange(windowSize);
        }

        public override void Update(Bar bar)
        {
            _sdtr.Update(bar);
            _atr.Update(bar);

            if (Math.Abs(_atr.Value) < 1e-6)
            {
                SetValue(0.0);
            }
            else
            {
                SetValue(_sdtr.Value / _atr.Value);
            }
        }
    }
}
