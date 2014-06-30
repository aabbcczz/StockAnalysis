using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("OSC")]
    public sealed class Oscillator : SingleOutputRawInputSerialMetric
    {
        private MovingAverage _ma;

        public Oscillator(int windowSize)
            : base(1)
        {
            _ma = new MovingAverage(windowSize);
        }

        public override double Update(double dataPoint)
        {
            return dataPoint - _ma.Update(dataPoint);
        }
    }
}
