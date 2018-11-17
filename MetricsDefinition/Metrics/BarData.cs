using StockAnalysis.Common.Data;

namespace MetricsDefinition.Metrics
{
    [Metric("BAR", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class BarData : MultipleOutputBarInputSerialMetric
    {
        public BarData()
            : base(0)
        {
            Values = new double[6];
        }

        public override void Update(Bar bar)
        {
            SetValue(
                bar.ClosePrice,
                bar.OpenPrice,
                bar.HighestPrice,
                bar.LowestPrice,
                bar.Volume,
                bar.Amount);
        }
    }
}
