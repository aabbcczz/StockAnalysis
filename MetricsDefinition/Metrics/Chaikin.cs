namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;
    using Common.Data;

    /// <summary>
    /// The Chavkin metric
    /// </summary>
    [Metric("CV")]
    public sealed class Chaikin : SingleOutputBarInputSerialMetric
    {
        private readonly int _interval;
        private readonly ExponentialMovingAverage _ema;
        private readonly CirculatedArray<double> _mahl;

        public Chaikin(int windowSize, int interval)
            : base(0)
        {
            if (interval <= 0 || interval > windowSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            _interval = interval;

            _ema = new ExponentialMovingAverage(windowSize);
            _mahl = new CirculatedArray<double>(interval);
        }

        public override void Update(Bar bar)
        {
            _ema.Update(bar.HighestPrice - bar.LowestPrice);
            var mahl = _ema.Value;

            _mahl.Add(mahl);

            var index = _mahl.Length < _interval ? 0 : _mahl.Length - _interval;

            var chaikin = Math.Abs(_mahl[index]) < 1e-6
                ? 0.0
                : (_mahl[-1] - _mahl[index]) / _mahl[index] * 100.0;

            SetValue(chaikin);
        }
    }
}
