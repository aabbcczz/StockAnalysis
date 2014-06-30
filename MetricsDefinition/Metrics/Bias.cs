using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("BIAS")]
    public sealed class Bias : SingleOutputRawInputSerialMetric
    {
        private MovingAverage _ma;

        public Bias(int windowSize)
            : base(1)
        {
            _ma = new MovingAverage(windowSize);
        }

        public override double Update(double dataPoint)
        {
            double average = _ma.Update(dataPoint);

            return (dataPoint - average) / average;
        }
    }
}
