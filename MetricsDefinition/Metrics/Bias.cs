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

        public override double Update(double dataPoint)
        {
            var average = _ma.Update(dataPoint);

            return (dataPoint - average) / average;
        }
    }
}
