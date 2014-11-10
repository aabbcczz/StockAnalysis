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
            Breakthrough,
            Bounce,
        }

        private PriceState _state;

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

        private void SetBreakthroughState(double lowestPrice)
        {
            _state = PriceState.Breakthrough;
            LowestPrice = lowestPrice;
            BouncePrice = 0.0;
            BouncePercentage = 0.0;
            _minBouncePrice = LowestPrice * (1 + _minBouncePercentage / 100.0);
        }

        private void UpdateState(Bar bar)
        {
            _lowest.Update(bar.ClosePrice);
            double lowestPrice = _lowest.Value;

            bool breakthrough = Math.Abs(lowestPrice - bar.ClosePrice) < 1e-6;

            switch(_state)
            {
                case PriceState.Initial:
                    if (breakthrough)
                    {
                        SetBreakthroughState(lowestPrice);
                    }

                    break;
                case PriceState.Breakthrough:
                    if (breakthrough)
                    {
                        SetBreakthroughState(lowestPrice);
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
                    if (breakthrough)
                    {
                        SetBreakthroughState(lowestPrice);
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
