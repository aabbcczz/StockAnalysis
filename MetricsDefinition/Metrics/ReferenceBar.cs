namespace StockAnalysis.MetricsDefinition.Metrics
{
    using Common.Data;

    [Metric("REFBAR", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class ReferenceBar : MultipleOutputBarInputSerialMetric
    {
        public ReferenceBar(int reference)
            : base(reference)
        {
            Values = new double[6];
        }

        public override void Update(Bar bar)
        {
            if (Data.Length == 0)
            {
                SetValue(0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            }
            else
            {
                Bar refBar = Data[0];
                SetValue(
                    refBar.ClosePrice,
                    refBar.OpenPrice,
                    refBar.HighestPrice,
                    refBar.LowestPrice,
                    refBar.Volume,
                    refBar.Amount);
            }

            Data.Add(bar);
        }
    }
}
