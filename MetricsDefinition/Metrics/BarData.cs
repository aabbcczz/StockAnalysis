using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("BAR", "CP,OP,HP,LP,VOL,AMT")]
    public sealed class BarData : MultipleOutputBarInputSerialMetric
    {
        public const int FieldCount = 6;
        public const int ClosePriceFieldIndex = 0;
        public const int OpenPriceFieldIndex = 1;
        public const int HighestPriceFieldIndex = 2;
        public const int LowestPriceFieldIndex = 3;
        public const int VolumeFieldIndex = 4;
        public const int AmountFieldIndex = 5;

        public BarData()
            : base(1)
        {
        }

        public override double[] Update(Bar bar)
        {
            return new double[FieldCount] 
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
