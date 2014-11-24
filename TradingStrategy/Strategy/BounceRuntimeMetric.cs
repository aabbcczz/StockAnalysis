using System;
using MetricsDefinition.Metrics;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public sealed class BounceRuntimeMetric : IRuntimeMetric
    {
        private readonly Lowest _lowest;

        private readonly double _minBouncePercentage;

        private double _minBouncePrice;

        private enum PriceState
        {
            Initial,
            Breakout,
            Bounce,
        }

        private PriceState _state;

        public double[] Values
        {
            get { return null; }
        }

        public double LowestPrice { get; private set; }

        public double BouncePrice { get; private set; }

        public double BouncePercentage { get; private set; }

        public bool Triggered
        {
            get { return _state == PriceState.Bounce; }
        }

        public BounceRuntimeMetric(int windowSize, double minBouncePercentage)
        {
            _lowest = new Lowest(windowSize);
            _minBouncePercentage = minBouncePercentage;

            ResetState();
        }

        private void ResetState()
        {
            _state = PriceState.Initial;
            LowestPrice = 0.0;
            BouncePrice = 0.0;
            BouncePercentage = 0.0;
            _minBouncePrice = 0.0;
        }

        private void SetBreakoutState(double lowestPrice)
        {
            _state = PriceState.Breakout;
            LowestPrice = lowestPrice;
            BouncePrice = 0.0;
            BouncePercentage = 0.0;
            _minBouncePrice = LowestPrice * (1 + _minBouncePercentage / 100.0);
        }

        private void UpdateState(Bar bar)
        {
            _lowest.Update(bar.ClosePrice);
            double lowestPrice = _lowest.Value;

            bool breakout = Math.Abs(lowestPrice - bar.ClosePrice) < 1e-6;

            switch(_state)
            {
                case PriceState.Initial:
                    if (breakout)
                    {
                        SetBreakoutState(lowestPrice);
                    }

                    break;
                case PriceState.Breakout:
                    if (breakout)
                    {
                        SetBreakoutState(lowestPrice);
                    }
                    else
                    {
                        if (bar.ClosePrice > _minBouncePrice)
                        {
                            _state = PriceState.Bounce;
                            BouncePrice = bar.ClosePrice;
                            BouncePercentage = (bar.ClosePrice - LowestPrice) / LowestPrice * 100.0;
                        }
                    }

                    break;
                case PriceState.Bounce:
                    if (breakout)
                    {
                        SetBreakoutState(lowestPrice);
                    }
                    else
                    {
                        ResetState();
                    }

                    break;
            }
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            UpdateState(bar);
        }
    }
}
