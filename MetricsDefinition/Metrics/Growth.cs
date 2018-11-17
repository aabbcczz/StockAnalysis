using StockAnalysis.Common.Data;

namespace MetricsDefinition.Metrics
{
    /// <summary>
    /// The ATR metric
    /// </summary>
    [Metric("GROWTH")]
    public sealed class Growth : SingleOutputBarInputSerialMetric
    {
        public Growth(int windowSize)
            : base(windowSize)
        {
        }

        public override void Update(Bar bar)
        {
            Data.Add(bar);

            Bar bar0 = Data[0];
            Bar barLast = Data[-1];

            var growth = (barLast.ClosePrice - bar0.OpenPrice) / bar0.OpenPrice;

            SetValue(growth);
        }
    }
}
