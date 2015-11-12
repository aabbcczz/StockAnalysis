using System;
using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("BARM", "UPSHADOW,DOWNSHADOW,CHANGERATIO,CHANGERATIOOVERPREVLOWEST,UPFORCE")]
    public sealed class BarMetric : MultipleOutputBarInputSerialMetric
    {
        private readonly double _alpha;

        public BarMetric(double alpha)
            : base(1)
        {
            if (alpha < 0.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException("alpha must be in [0.0..1.0]");
            }

            _alpha = alpha;

            Values = new double[5];

        }

        public override void Update(Bar bar)
        {
            double wholeBarLength = bar.HighestPrice - bar.LowestPrice;

            if (Math.Abs(wholeBarLength) < 1e-6)
            {
                SetValue(0.0, 0.0, 0.0, 0.0,0.0);
            }

            double upShadow = (bar.HighestPrice - Math.Max(bar.OpenPrice, bar.ClosePrice)) / wholeBarLength;
            double downShadow = (Math.Min(bar.OpenPrice, bar.ClosePrice) - bar.LowestPrice) / wholeBarLength;
            double changeRatio = (bar.ClosePrice - bar.OpenPrice) / bar.OpenPrice;
            double changeRatioOverPreviousLowest = 0.0;
            double upForce = 0.0;

            if (Data.Length > 0)
            {
                var previousLowest = Data[0].LowestPrice;
                changeRatioOverPreviousLowest = Math.Abs(previousLowest) < 1e-6
                    ? 0.0
                    : (bar.ClosePrice - previousLowest) / previousLowest;

                double lowestRatioOverPreviousLowest = Math.Abs(previousLowest) < 1e-6
                    ? 0.0
                    : (bar.LowestPrice - previousLowest) / previousLowest;

                upForce = _alpha * changeRatioOverPreviousLowest + (1.0 - _alpha) * lowestRatioOverPreviousLowest;
            }

            SetValue(upShadow, downShadow, changeRatio, changeRatioOverPreviousLowest, upForce);

            Data.Add(bar);
        }
    }
}
