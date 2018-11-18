using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("TESTMETRIC", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class _TestMetric : MultipleOutputBarInputSerialMetric
    {
        public _TestMetric(int p1, string p2, float p3)
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
