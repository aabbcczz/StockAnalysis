using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class MovingAverageRuntimeMetric : IRuntimeMetric
    {
        public double ShortMa { get; private set; }

        public double LongMa { get; private set; }

        public double PreviousShortMa { get; private set; }

        public double PreviousLongMa { get; private set; }

        private readonly MovingAverage _short;
        private readonly MovingAverage _long;

        public MovingAverageRuntimeMetric(int shortPeriod, int longPeriod)
        {
            _short = new MovingAverage(shortPeriod);
            _long = new MovingAverage(longPeriod);
        }

        public void Update(Bar bar)
        {
            PreviousLongMa = LongMa;
            PreviousShortMa = ShortMa;

            ShortMa = _short.Update(bar.ClosePrice);
            LongMa = _long.Update(bar.ClosePrice);
        }
    }
}
