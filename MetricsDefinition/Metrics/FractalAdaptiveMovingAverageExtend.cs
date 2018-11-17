using System;
using StockAnalysis.Common.Data;

namespace MetricsDefinition.Metrics
{
    // please refer to http://etfhq.com/blog/2010/09/30/fractal-adaptive-moving-average-frama/
    // for more details and understand the algorithm

    [Metric("FRAMA_EX")]
    public sealed class FractalAdaptiveMovingAverageExtend : SingleOutputBarInputSerialMetric
    {
        private readonly int _sc;
        private readonly int _fc;
        private readonly int _h;

        private readonly int _period;
        private readonly int _halfPeriod;

        private readonly double _w;
        private readonly double _minAlpha;
        private readonly double _maxAlpha;
        private readonly double _log2 = Math.Log(2.0);

        private double _lastFrama = double.NaN;

        private readonly MovingAverage _initialFrama;
        private readonly Highest _firstHalfHighest;
        private readonly Lowest _firstHalfLowest;
        private readonly Highest _secondHalfHighest;
        private readonly Lowest _secondHalfLowest;

        public FractalAdaptiveMovingAverageExtend(int period, int fastMovingAverage, int slowMovingAverage)
            : base(0)
        {
            if (period <= 0)
            {
                throw new ArgumentException("period must be greater than 0");
            }

            if (period % 2 != 0)
            {
                throw new ArgumentException("period must be even number");
            }

            if (slowMovingAverage <= 0 || fastMovingAverage <= 0)
            {
                throw new ArgumentException("slow moving average or fast moving average must be greather than 0");
            }

            if (fastMovingAverage >= slowMovingAverage)
            {
                throw new ArgumentException("fast moving average must be smaller than slow moving average");
            }

            _sc = slowMovingAverage;
            _fc = fastMovingAverage;
            _h = Math.Max(_period - 1, Even((_sc - _fc) / 2) + _fc);
            _period = period;
            _halfPeriod = period / 2;
            _minAlpha = 2.0 / (_sc + 1);
            _maxAlpha = 1.0;
            _w = Math.Log(_minAlpha);

            _initialFrama = new MovingAverage(_h);
            _firstHalfHighest = new Highest(_halfPeriod);
            _firstHalfLowest = new Lowest(_halfPeriod);
            _secondHalfHighest = new Highest(_halfPeriod);
            _secondHalfLowest = new Lowest(_halfPeriod);
        }

        private static int Even(int x)
        {
            if (x % 2 == 0)
            {
                return x;
            }

            if (x > 0)
            {
                return x + 1;
            }
            else
            {
                return x - 1;
            }
        }

        public override void Update(Bar bar)
        {
            if (_secondHalfHighest.Data.Length < _halfPeriod)
            {
                // second half is not full yet
                if (_firstHalfHighest.Data.Length < _halfPeriod)
                {
                    // first half is not full yet
                    _firstHalfHighest.Update(bar.HighestPrice);
                    _firstHalfLowest.Update(bar.LowestPrice);
                }
                else
                {
                    // first half is full
                    _secondHalfHighest.Update(bar.HighestPrice);
                    _secondHalfLowest.Update(bar.LowestPrice);
                }
            }
            else
            {
                _firstHalfHighest.Update(_secondHalfHighest.Data[0]);
                _firstHalfLowest.Update(_secondHalfLowest.Data[0]);

                _secondHalfHighest.Update(bar.HighestPrice);
                _secondHalfLowest.Update(bar.LowestPrice);
            }

            // warming up stage
            if (double.IsNaN(_lastFrama))
            {
                _initialFrama.Update(bar.ClosePrice);

                if (_initialFrama.Data.Length >= _h)
                {
                    _lastFrama = _initialFrama.Value;
                }

                SetValue(_initialFrama.Value);

                return;
            }

            // calculation stage
            double firstHalfHighestValue = _firstHalfHighest.Value;
            double firstHalfLowestValue = _firstHalfLowest.Value;
            double secondHalfHighestValue = _secondHalfHighest.Value;
            double secondHalfLowestValue = _secondHalfLowest.Value;

            double highestValue = Math.Max(firstHalfHighestValue, secondHalfHighestValue);
            double lowestValue = Math.Min(firstHalfLowestValue, secondHalfLowestValue);

            var hl1 = (firstHalfHighestValue - firstHalfLowestValue) / _halfPeriod;
            var hl2 = (secondHalfHighestValue - secondHalfLowestValue) / _halfPeriod;
            var hl = (highestValue - lowestValue) / _period;

            var fractalDimension = (Math.Log(hl1 + hl2) - Math.Log(hl)) / _log2;
            var alpha = Math.Exp(_w * (fractalDimension - 1.0));

            if (alpha < _minAlpha)
            {
                alpha = _minAlpha;
            }

            if (alpha > _maxAlpha)
            {
                alpha = _maxAlpha;
            }

            var originalNEma = (2.0 - alpha) / alpha;
            var newNEma = ((_sc - _fc) * ((originalNEma - 1.0) / (_sc - 1.0))) + _fc;
            var newAlpha = 2.0 / (newNEma + 1.0);

            var newFrama = _lastFrama + newAlpha * (bar.ClosePrice - _lastFrama);

            SetValue(newFrama);

            _lastFrama = newFrama;
        }
    }
}
