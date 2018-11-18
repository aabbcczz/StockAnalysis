namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;

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

        public override void Update(double dataPoint)
        {
            _volatility.Update(Math.Abs(dataPoint - _previousData));
            var volatilitySum = _volatility.Value;

            var movingSpeed = Data.Length == 0 ? 0.0 : Math.Abs(Data[-1] - Data[0]);

            Data.Add(dataPoint);

            _previousData = dataPoint;

            var efficencyRatio = Math.Abs(volatilitySum) < 1e-6 ? 0.0: movingSpeed / volatilitySum;

            SetValue(efficencyRatio);
        }
     }
}
