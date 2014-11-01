using System;
using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class BreakthroughAndReturnRuntimeMetric : IRuntimeMetric
    {
        private readonly Highest _highest;

        private readonly int _maxInterval;
        private readonly int _minInterval;

        private int _intervalBetweenLastBreakthroughAndRerising;

        private enum PriceState
        {
            Initial,
            Breakthrough,
            Degrading,
            Rising,
        }

        private PriceState _state;

        public double LatestBreakthroughPrice { get; private set; }

        public double LowestPriceAfterBreakthrough { get; private set; }

        public bool Triggered
        {
            get { return _state == PriceState.Rising; }
        }

        public BreakthroughAndReturnRuntimeMetric(int windowSize, int maxInterval, int minInterval)
        {
            _highest = new Highest(windowSize);
            _maxInterval = maxInterval;
            _minInterval = minInterval;

            ResetState();
        }

        private void ResetState()
        {
            _state = PriceState.Initial;
            LatestBreakthroughPrice = 0.0;
            LowestPriceAfterBreakthrough = 0.0;
            _intervalBetweenLastBreakthroughAndRerising = 0;
        }
        private void UpdateState(Bar bar)
        {
            double highestPrice = _highest.Update(bar.HighestPrice);

            bool breakthrough = Math.Abs(highestPrice - bar.HighestPrice) < 1e-6;

            switch(_state)
            {
                case PriceState.Initial:
                    if (breakthrough)
                    {
                        _state = PriceState.Breakthrough;
                        LatestBreakthroughPrice = highestPrice;
                        LowestPriceAfterBreakthrough = 0.0;
                        _intervalBetweenLastBreakthroughAndRerising = 0;
                    }

                    break;
                case PriceState.Breakthrough:
                    if (breakthrough)
                    {
                        LatestBreakthroughPrice = highestPrice;
                    }
                    else
                    {
                        _state = PriceState.Degrading;
                        LowestPriceAfterBreakthrough = bar.ClosePrice;
                        _intervalBetweenLastBreakthroughAndRerising = 1;
                    }

                    break;
                case PriceState.Degrading:
                    if (bar.ClosePrice <= LowestPriceAfterBreakthrough)
                    {
                        LowestPriceAfterBreakthrough = bar.ClosePrice;
                        _intervalBetweenLastBreakthroughAndRerising++;
                    }
                    else
                    {
                        if (_intervalBetweenLastBreakthroughAndRerising >= _minInterval 
                            && _intervalBetweenLastBreakthroughAndRerising <= _maxInterval)
                        {
                            _state = PriceState.Rising;
                        }
                        else
                        {
                            ResetState();
                        }
                    }
                    break;
                case PriceState.Rising:
                    ResetState();
                    break;
            }
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            UpdateState(bar);
        }
    }
}
