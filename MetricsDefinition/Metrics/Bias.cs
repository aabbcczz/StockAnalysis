namespace MetricsDefinition.Metrics
{
    [Metric("BIAS")]
    public sealed class Bias : SingleOutputRawInputSerialMetric
    {
        private readonly MovingAverage _ma;

        public Bias(int windowSize)
            : base(0)
        {
            _ma = new MovingAverage(windowSize);
        }

        public override void Update(double dataPoint)
        {
            _ma.Update(dataPoint);
            var average = _ma.Value;

            SetValue((dataPoint - average) / average);
        }
    }
}
