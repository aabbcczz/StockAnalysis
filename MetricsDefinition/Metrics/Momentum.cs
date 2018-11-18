using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("MTM")]
    public sealed class Momentum : SingleOutputRawInputSerialMetric
    {
         public Momentum(int windowSize)
             : base(windowSize + 1)
        {
        }

         public override void Update(double dataPoint)
         {
             Data.Add(dataPoint);

             var oldData = Data[0];

             var mtm = Math.Abs(oldData) < 1e-6 ? 0.0 : dataPoint * 100.0 / oldData;

             SetValue(mtm);
         }
    }
}
