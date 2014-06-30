using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("EMA, EXPMA")]
    public sealed class ExponentialMovingAverage : SingleOutputRawInputSerialMetric
    {
        private double _lastResult = 0.0;

        public ExponentialMovingAverage(int windowSize)
            : base(windowSize)
        {
            if (windowSize < 2)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than 1");
            }
        }

        public override double Update(double dataPoint)
        {
            Data.Add(dataPoint);

            if (Data.Length == 1)
            {
                _lastResult = dataPoint;
            }
            else
            {
                _lastResult = (_lastResult * (WindowSize - 1) + dataPoint * 2.0) / (WindowSize + 1);
            }

            return _lastResult;
        }
    }
}
