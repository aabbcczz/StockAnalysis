using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MA")]
    public sealed class MovingAverage : SingleOutputRawInputSerialMetric
    {
        private MovingSum _movingSum;

        public MovingAverage(int windowSize)
            : base(1)
        {
            _movingSum = new MovingSum(windowSize);
        }

        public override double Update(double dataPoint)
        {
            double sum = _movingSum.Update(dataPoint);
            
            return sum / _movingSum.Data.Length;
        }
    }
}
