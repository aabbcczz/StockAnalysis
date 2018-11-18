namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;

    [Metric("EMA, EXPMA")]
    public sealed class ExponentialMovingAverage : SingleOutputRawInputSerialMetric
    {
        private double _lastResult;

        public ExponentialMovingAverage(int windowSize)
            : base(windowSize)
        {
            if (windowSize < 2)
            {
                throw new ArgumentOutOfRangeException("windowSize must be greater than 1");
            }
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);

            if (Data.Length == 1)
            {
                _lastResult = dataPoint;
            }
            else
            {
                _lastResult = (_lastResult * (WindowSize - 1) + dataPoint * 2.0) / (WindowSize + 1);
            }

            SetValue(_lastResult);
        }
    }
}
