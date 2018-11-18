using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("ROC")]
    public sealed class RateOfChange : SingleOutputRawInputSerialMetric
    {
        public RateOfChange(int windowSize)
            : base (windowSize + 1)
        {
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);

            var oldData = Data[0];

            var roc = Math.Abs(oldData) < 1e-6 ? 0.0 : (dataPoint - oldData) / oldData * 100.0;

            SetValue(roc);
        }
    }
}
