using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("BAR", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class BarData : MultipleOutputBarInputSerialMetric
    {
        public BarData()
            : base(0)
        {
        }

        public override double[] Update(Bar bar)
        {
            return new[] 
            { 
                bar.ClosePrice,
                bar.OpenPrice,
                bar.HighestPrice,
                bar.LowestPrice,
                bar.Volume,
                bar.Amount
            };
        }
    }
}
