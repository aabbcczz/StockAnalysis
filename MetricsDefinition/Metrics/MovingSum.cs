using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MS")]
    public sealed class MovingSum : SingleOutputRawInputSerialMetric
    {
        private double _sum = 0.0;

        public MovingSum(int windowSize)
            : base(windowSize)
        {
        }

        public override double Update(double dataPoint)
        {
            _sum += dataPoint;

            if (Data.Length >= WindowSize)
            {
                _sum -= Data[0];
            }

            Data.Add(dataPoint);

            return _sum;
        }

    }
}
