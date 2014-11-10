namespace MetricsDefinition.Metrics
{
    [Metric("MA")]
    public sealed class MovingAverage : SingleOutputRawInputSerialMetric
    {
        private readonly MovingSum _movingSum;

        public MovingAverage(int windowSize)
            : base(0)
        {
            _movingSum = new MovingSum(windowSize);
        }

        public override void Update(double dataPoint)
        {
            _movingSum.Update(dataPoint);
            var sum = _movingSum.Value;
            
            SetValue(sum / _movingSum.Data.Length);
        }
    }
}
