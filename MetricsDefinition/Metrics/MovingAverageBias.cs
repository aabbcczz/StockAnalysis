namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;

    [Metric("MB,MAB")]
    public sealed class MovingAverageBias : SingleOutputRawInputSerialMetric
    {
        private readonly MovingAverage _maShort;
        private readonly MovingAverage _maLong;

        public MovingAverageBias(int shortWindowSize, int longWindowSize)
            : base(0)
        {
            if (shortWindowSize <= 0 || longWindowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("windowSize");
            }

            if (shortWindowSize >= longWindowSize)
            {
                throw new ArgumentException("short windowSize should be smaller than long windowSize");
            }

            _maShort = new MovingAverage(shortWindowSize);
            _maLong = new MovingAverage(longWindowSize);
        }

        public override void Update(double dataPoint)
        {
            _maShort.Update(dataPoint);
            _maLong.Update(dataPoint);

            SetValue(_maShort.Value - _maLong.Value);
        }
    }
}
