using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("MB,MAB")]
    public sealed class MovingAverageBias : SingleOutputRawInputSerialMetric
    {
        private MovingAverage _maShort;
        private MovingAverage _maLong;

        public MovingAverageBias(int shortWindowSize, int longWindowSize)
            : base(1)
        {
            if (shortWindowSize <= 0 || longWindowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("windowSize");
            }

            if (shortWindowSize >= longWindowSize)
            {
                throw new ArgumentException("short windowSize should be smaller than long windowSize");
            }

            _maShort = new MovingAverage(shortWindowSize);
            _maLong = new MovingAverage(longWindowSize);
        }

        public override double Update(double dataPoint)
        {
            return _maShort.Update(dataPoint) - _maLong.Update(dataPoint);
        }
    }
}
