using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("BOLL", "UB,MA,LB")]
    public sealed class BollingerBand : MultipleOutputRawInputSerialMetric
    {
        private double _alpha;

        private MovingAverage _ma;
        private StdDev _sd;

        public BollingerBand(int windowSize, double alpha)
            : base(1)
        {
            if (alpha <= 0.0)
            {
                throw new ArgumentOutOfRangeException("alpha");
            }

            _alpha = alpha;

            _ma = new MovingAverage(windowSize);
            _sd = new StdDev(windowSize);
        }

        public override double[] Update(double dataPoint)
        {
            double ma = _ma.Update(dataPoint);
            double stddev = _sd.Update(dataPoint);

            double upperBound = ma + _alpha * stddev;
            double lowerBound = ma - _alpha * stddev;

            return new double[3] { upperBound, ma, lowerBound };
        }
    }
}
