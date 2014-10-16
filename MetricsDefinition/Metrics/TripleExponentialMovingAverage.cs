namespace MetricsDefinition.Metrics
{
    [Metric("TRIX,TEMA")]
    public sealed class TripleExponentialMovingAverage : SingleOutputRawInputSerialMetric
    {
        private readonly ExponentialMovingAverage _ema1;
        private readonly ExponentialMovingAverage _ema2;
        private readonly ExponentialMovingAverage _ema3;

        public TripleExponentialMovingAverage(int windowSize)
            : base(1)
        {
            _ema1 = new ExponentialMovingAverage(windowSize);
            _ema2 = new ExponentialMovingAverage(windowSize);
            _ema3 = new ExponentialMovingAverage(windowSize);
        }

        public override double Update(double dataPoint)
        {
            return _ema1.Update(_ema2.Update(_ema3.Update(dataPoint)));
        }
    }
}
