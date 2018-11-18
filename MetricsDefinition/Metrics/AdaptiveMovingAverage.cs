namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;

    [Metric("AMA")]
    public sealed class AdaptiveMovingAverage : SingleOutputRawInputSerialMetric
    {
        private const int MinWindowSize = 3;
        private const double FastSmoothCoefficient = 2.0 / (2.0 + 1.0);

        private readonly double _slowSmoothCoefficient;
        private readonly double _smoothCoefficientDifference;

        private readonly EfficiencyRatio _efficiencyRatio;
        private double _lastAma = 0.0;

        public AdaptiveMovingAverage(int windowSize)
            : base(0)
        {
            if (windowSize < MinWindowSize)
            {
                throw new ArgumentException(string.Format("window size of adaptive moving average must be greater than {0}", MinWindowSize - 1));
            }

            _slowSmoothCoefficient = 2.0 / (1.0 + windowSize);
            _smoothCoefficientDifference = FastSmoothCoefficient - _slowSmoothCoefficient;

            _efficiencyRatio = new EfficiencyRatio(windowSize);
        }

        public override void Update(double dataPoint)
        {
            _efficiencyRatio.Update(dataPoint);
            var efficiencyRatio = _efficiencyRatio.Value;

            var smoothCoefficient = efficiencyRatio * _smoothCoefficientDifference + _slowSmoothCoefficient;

            var ama = _lastAma + smoothCoefficient * smoothCoefficient * (dataPoint - _lastAma);

            _lastAma = ama;

            SetValue(ama);
        }
    }
}
