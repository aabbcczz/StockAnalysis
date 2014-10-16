using System;

namespace MetricsDefinition.Metrics
{
    [Metric("MTM")]
    public sealed class Momentum : SingleOutputRawInputSerialMetric
    {
         public Momentum(int windowSize)
             : base(windowSize + 1)
        {
        }

         public override double Update(double dataPoint)
         {
             Data.Add(dataPoint);

             var oldData = Data[0];

             return Math.Abs(oldData) < 1e-6 ? 0.0 : dataPoint * 100.0 / oldData;
         }
    }
}
