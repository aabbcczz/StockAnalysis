using System;
using MetricsDefinition.Metrics;

namespace TradingStrategy.Strategy
{
    public sealed class RebreakthroughRuntimeMetric : IRuntimeMetric
    {
        private readonly Highest _highest;
        private readonly int _priceSelector;

        private readonly int _maxInterval;
        private readonly int _minInterval;

        private int _intervalSinceLastBreakthrough;

        public double CurrentHighest { get; private set; }

        public bool Breakthrough { get; private set; }

        public bool Rebreakthrough { get; private set; }

        public int IntervalSinceLastBreakthrough { get; private set; }

        public RebreakthroughRuntimeMetric(int windowSize, int priceSelector, int maxInterval, int minInterval)
        {
            _highest = new Highest(windowSize);
            _priceSelector = priceSelector;
            _maxInterval = maxInterval;
            _minInterval = minInterval;

            CurrentHighest = 0.0;
            Breakthrough = false;
            Rebreakthrough = false;
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            double price = BarPriceSelector.Select(bar, _priceSelector);

            _highest.Update(price);
            double newHighest = _highest.Value;

            bool oldBreakthrough = Breakthrough;

            Breakthrough = Math.Abs(newHighest - price) < 1e-6;

            CurrentHighest = newHighest;

            if (Breakthrough)
            {
                // rebreakthrough is always breakthrough
                if (oldBreakthrough)
                {
                    // continuous breakthrough is not rebreakthrough
                    Rebreakthrough = false;
                    _intervalSinceLastBreakthrough = 0;
                }
                else
                {
                    // possible a rebreakthrough.
                    if (_intervalSinceLastBreakthrough > 0 
                        && _intervalSinceLastBreakthrough <= _maxInterval
                        && _intervalSinceLastBreakthrough >= _minInterval)
                    {
                        Rebreakthrough = true;
                        IntervalSinceLastBreakthrough = _intervalSinceLastBreakthrough;

                        _intervalSinceLastBreakthrough = 0;
                    }
                    else
                    {
                        Rebreakthrough = false;
                        _intervalSinceLastBreakthrough = 0;
                    }
                }
            }
            else
            {
                // rebreakthrough is always breakthrough
                Rebreakthrough = false;

                if (oldBreakthrough)
                {
                    _intervalSinceLastBreakthrough = 1;
                }
                else
                {
                    if (_intervalSinceLastBreakthrough > 0)
                    {
                        _intervalSinceLastBreakthrough++;
                    }
                }
            }
        }
    }
}
