using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("PC")]
    public sealed class PriceChange : SingleOutputBarInputSerialMetric
    {
        public PriceChange()
            : base(0)
        {
        }

        public override void Update(Bar bar)
        {
            var change = (bar.ClosePrice - bar.OpenPrice) / bar.OpenPrice * 100.0;

            SetValue(change);
        }
    }
}
