using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("PSY")]
    public sealed class PsychologicalLine : SingleOutputRawInputSerialMetric
    {
        private double _prevData;
        private bool _firstData = true;

        private MovingAverage _ma;

        public PsychologicalLine(int windowSize)
            : base(1)
        {
            _ma = new MovingAverage(windowSize);
        }

        public override double Update(double dataPoint)
        {
            double up = _firstData
                ? 0.0
                : (dataPoint > _prevData)
                    ? 100.0
                    : 0.0;

            // update status
            _prevData = dataPoint;
            _firstData = false;

            // return result
            return _ma.Update(up);
        }
    }
}
