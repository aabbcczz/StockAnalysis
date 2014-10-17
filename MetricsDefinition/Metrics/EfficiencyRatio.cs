using System;

namespace MetricsDefinition.Metrics
{
    [Metric("ER")]
    public sealed class EfficiencyRatio : SingleOutputRawInputSerialMetric
    {
        private double _previousData;
        private readonly MovingSum _volatility;

        public EfficiencyRatio(int windowSize)
            : base(windowSize)
        {
            _volatility = new MovingSum(windowSize);
        }

        public override double Update(double dataPoint)
        {
            var volatilitySum = _volatility.Update(Math.Abs(dataPoint - _previousData));

            var movingSpeed = Data.Length == 0 ? 0.0 : Data[-1] - Data[0];

            Data.Add(dataPoint);

            _previousData = dataPoint;

            return Math.Abs(volatilitySum) < 1e-6 ? 0.0: movingSpeed / volatilitySum;
        }
     }
}
