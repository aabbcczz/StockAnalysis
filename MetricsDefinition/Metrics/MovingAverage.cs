namespace MetricsDefinition.Metrics
{
    [Metric("MA")]
    public sealed class MovingAverage : SingleOutputRawInputSerialMetric
    {
        private readonly MovingSum _movingSum;

        public MovingAverage(int windowSize)
            : base(1)
        {
            _movingSum = new MovingSum(windowSize);
        }

        public override double Update(double dataPoint)
        {
            var sum = _movingSum.Update(dataPoint);
            
            return sum / _movingSum.Data.Length;
        }
    }
}
