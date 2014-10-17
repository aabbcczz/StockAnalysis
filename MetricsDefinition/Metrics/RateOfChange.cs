using System;

namespace MetricsDefinition.Metrics
{
    [Metric("ROC")]
    public sealed class RateOfChange : SingleOutputRawInputSerialMetric
    {
        public RateOfChange(int windowSize)
            : base (windowSize + 1)
        {
        }

        public override double Update(double dataPoint)
        {
            Data.Add(dataPoint);

            var oldData = Data[0];

            return Math.Abs(oldData) < 1e-6 ? 0.0 : (dataPoint - oldData) / oldData * 100.0;
        }
    }
}
