using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("BARM", "UPSHADOW,DOWNSHADOW")]
    public sealed class BarMetric : MultipleOutputBarInputSerialMetric
    {
        public BarMetric()
            : base(0)
        {
            Values = new double[2];
        }

        public override void Update(Bar bar)
        {
            double wholeBarLength = bar.HighestPrice - bar.LowestPrice;

            if (Math.Abs(wholeBarLength) < 1e-6)
            {
                SetValue(0.0, 0.0);
            }

            double upShadow = (bar.HighestPrice - Math.Max(bar.OpenPrice, bar.ClosePrice)) / wholeBarLength;
            double downShadow = (Math.Min(bar.OpenPrice, bar.ClosePrice) - bar.LowestPrice) / wholeBarLength;

            SetValue(upShadow, downShadow);
        }
    }
}
