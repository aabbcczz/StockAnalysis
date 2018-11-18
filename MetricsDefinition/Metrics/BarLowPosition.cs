using System;
using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    /// <summary>
    /// The Bar low position metric. Used to calculate the position in the low shade of bar.
    /// </summary>
    [Metric("BLP")]
    public sealed class BarLowPosition : SingleOutputBarInputSerialMetric
    {
        private readonly double _proportion;

        public BarLowPosition(double proportion)
            : base(0)
        {
            _proportion = proportion;
        }

        public override void Update(Bar bar)
        {
            var value = bar.LowestPrice * (1.0 - _proportion) + Math.Min(bar.OpenPrice, bar.ClosePrice) * _proportion;

            SetValue(value);
        }
    }
}
