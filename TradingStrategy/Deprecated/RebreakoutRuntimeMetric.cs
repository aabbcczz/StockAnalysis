using System;
using StockAnalysis.MetricsDefinition.Metrics;
using StockAnalysis.Common.Data;

namespace StockAnalysis.TradingStrategy.Strategy
{
    public sealed class RebreakoutRuntimeMetric : IRuntimeMetric
    {
        private readonly Highest _highest;
        private readonly int _priceSelector;

        private readonly int _maxInterval;
        private readonly int _minInterval;

        private int _intervalSinceLastBreakout;

        public double[] Values { get { return null; } }

        public double CurrentHighest { get; private set; }

        public bool Breakout { get; private set; }

        public bool Rebreakout { get; private set; }

        public int IntervalSinceLastBreakout { get; private set; }

        public RebreakoutRuntimeMetric(int windowSize, int priceSelector, int maxInterval, int minInterval)
        {
            _highest = new Highest(windowSize);
            _priceSelector = priceSelector;
            _maxInterval = maxInterval;
            _minInterval = minInterval;

            CurrentHighest = 0.0;
            Breakout = false;
            Rebreakout = false;
        }

        public void Update(Bar bar)
        {
            double price = BarPriceSelector.Select(bar, _priceSelector);

            _highest.Update(price);
            double newHighest = _highest.Value;

            bool oldBreakout = Breakout;

            Breakout = Math.Abs(newHighest - price) < 1e-6;

            CurrentHighest = newHighest;

            if (Breakout)
            {
                // rebreakout is always breakout
                if (oldBreakout)
                {
                    // continuous breakout is not rebreakout
                    Rebreakout = false;
                    _intervalSinceLastBreakout = 0;
                }
                else
                {
                    // possible a rebreakout.
                    if (_intervalSinceLastBreakout > 0 
                        && _intervalSinceLastBreakout <= _maxInterval
                        && _intervalSinceLastBreakout >= _minInterval)
                    {
                        Rebreakout = true;
                        IntervalSinceLastBreakout = _intervalSinceLastBreakout;

                        _intervalSinceLastBreakout = 0;
                    }
                    else
                    {
                        Rebreakout = false;
                        _intervalSinceLastBreakout = 0;
                    }
                }
            }
            else
            {
                // rebreakout is always breakout
                Rebreakout = false;

                if (oldBreakout)
                {
                    _intervalSinceLastBreakout = 1;
                }
                else
                {
                    if (_intervalSinceLastBreakout > 0)
                    {
                        _intervalSinceLastBreakout++;
                    }
                }
            }
        }
    }
}
