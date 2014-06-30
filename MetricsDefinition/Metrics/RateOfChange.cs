using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
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

            double oldData = Data[0];

            return oldData == 0.0 ? 0.0 : (dataPoint - oldData) / oldData * 100.0;
        }
    }
}
