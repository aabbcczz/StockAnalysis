using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("BOLL", "UB,MA,LB")]
    public sealed class BollingerBand : MultipleOutputRawInputSerialMetric
    {
        private readonly double _alpha;

        private readonly MovingAverage _ma;
        private readonly StdDev _sd;

        public BollingerBand(int windowSize, double alpha)
            : base(0)
        {
            if (alpha <= 0.0)
            {
                throw new ArgumentOutOfRangeException("alpha");
            }

            _alpha = alpha;

            _ma = new MovingAverage(windowSize);
            _sd = new StdDev(windowSize);

            Values = new double[3];
        }

        public override void Update(double dataPoint)
        {
            _ma.Update(dataPoint);
            var ma = _ma.Value;

            _sd.Update(dataPoint);
            var stddev = _sd.Value;

            var upperBound = ma + _alpha * stddev;
            var lowerBound = ma - _alpha * stddev;

            SetValue(upperBound, ma, lowerBound);
        }
    }
}
