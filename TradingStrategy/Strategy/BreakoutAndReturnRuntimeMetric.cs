using System;
using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class BreakoutAndReturnRuntimeMetric : IRuntimeMetric
    {
        private readonly Highest _highest;
        private readonly int _priceSelector;
        private readonly int _maxInterval;
        private readonly int _minInterval;

        private int _intervalBetweenLastBreakoutAndRerising;

        private enum PriceState
        {
            Initial,
            Breakout,
            Degrading,
            Rising,
        }

        private PriceState _state;

        public double[] Values { get { return null; } }

        public double LatestBreakoutPrice { get; private set; }

        public double LowestPriceAfterBreakout { get; private set; }

        public bool Triggered
        {
            get { return _state == PriceState.Rising; }
        }

        public BreakoutAndReturnRuntimeMetric(int windowSize, int priceSelector, int maxInterval, int minInterval)
        {
            _highest = new Highest(windowSize);
            _priceSelector = priceSelector;
            _maxInterval = maxInterval;
            _minInterval = minInterval;

            ResetState();
        }

        private void ResetState()
        {
            _state = PriceState.Initial;
            LatestBreakoutPrice = 0.0;
            LowestPriceAfterBreakout = 0.0;
            _intervalBetweenLastBreakoutAndRerising = 0;
        }
        private void UpdateState(Bar bar)
        {
            double price = BarPriceSelector.Select(bar, _priceSelector);

            _highest.Update(price);
            double highestPrice = _highest.Value;

            bool breakout = Math.Abs(highestPrice - price) < 1e-6;

            switch(_state)
            {
                case PriceState.Initial:
                    if (breakout)
                    {
                        _state = PriceState.Breakout;
                        LatestBreakoutPrice = highestPrice;
                        LowestPriceAfterBreakout = 0.0;
                        _intervalBetweenLastBreakoutAndRerising = 0;
                    }

                    break;
                case PriceState.Breakout:
                    if (breakout)
                    {
                        LatestBreakoutPrice = highestPrice;
                    }
                    else
                    {
                        _state = PriceState.Degrading;
                        LowestPriceAfterBreakout = bar.ClosePrice;
                        _intervalBetweenLastBreakoutAndRerising = 1;
                    }

                    break;
                case PriceState.Degrading:
                    if (bar.ClosePrice <= LowestPriceAfterBreakout)
                    {
                        LowestPriceAfterBreakout = bar.ClosePrice;
                        _intervalBetweenLastBreakoutAndRerising++;
                    }
                    else
                    {
                        if (_intervalBetweenLastBreakoutAndRerising >= _minInterval 
                            && _intervalBetweenLastBreakoutAndRerising <= _maxInterval)
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
