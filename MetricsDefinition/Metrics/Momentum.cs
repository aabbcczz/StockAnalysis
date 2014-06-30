using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
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

             double oldData = Data[0];

             return oldData == 0.0 ? 0.0 : dataPoint * 100.0 / oldData;
         }
    }
}
