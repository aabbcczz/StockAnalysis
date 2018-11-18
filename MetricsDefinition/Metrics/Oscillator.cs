namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("OSC")]
    public sealed class Oscillator : SingleOutputRawInputSerialMetric
    {
        private readonly MovingAverage _ma;

        public Oscillator(int windowSize)
            : base(0)
        {
            _ma = new MovingAverage(windowSize);
        }

        public override void Update(double dataPoint)
        {
            _ma.Update(dataPoint);

            SetValue(dataPoint - _ma.Value);
        }
    }
}
