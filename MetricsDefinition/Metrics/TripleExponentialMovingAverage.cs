namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("TRIX,TEMA")]
    public sealed class TripleExponentialMovingAverage : SingleOutputRawInputSerialMetric
    {
        private readonly ExponentialMovingAverage _ema1;
        private readonly ExponentialMovingAverage _ema2;
        private readonly ExponentialMovingAverage _ema3;

        public TripleExponentialMovingAverage(int windowSize)
            : base(0)
        {
            _ema1 = new ExponentialMovingAverage(windowSize);
            _ema2 = new ExponentialMovingAverage(windowSize);
            _ema3 = new ExponentialMovingAverage(windowSize);
        }

        public override void Update(double dataPoint)
        {
            _ema3.Update(dataPoint);
            _ema2.Update(_ema3.Value);
            _ema1.Update(_ema2.Value);

            SetValue(_ema1.Value);
        }
    }
}
