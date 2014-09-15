using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("SAR")]
    public sealed class StopAndReverse : SingleOutputBarInputSerialMetric
    {
        private Highest _highestMetric;
        private Lowest _lowestMetric;
        private double _initialAccelerateFactor;
        private double _accelerateFactorStep;
        private double _maxAccelerateFactor;

        private double _accelerateFactor;
        private double _highestPrice;
        private double _lowestPrice;

        private bool _initialized = false;
        private bool _ascending = false;

        private double _sar;
        private double _ep;

        public StopAndReverse(int windowSize, double accelerateFactor, double accelerateFactorStep, double maxAccelerateFactor)
            : base(windowSize)
        {
            if (windowSize <= 1 
                || accelerateFactor <= 0.0 
                || accelerateFactorStep <= 0.0 
                || maxAccelerateFactor <= accelerateFactor)
            {
                throw new ArgumentException();
            }

            _highestMetric = new Highest(windowSize);
            _lowestMetric = new Lowest(windowSize);

            _initialAccelerateFactor = accelerateFactor;
            _accelerateFactorStep = accelerateFactorStep;
            _maxAccelerateFactor = maxAccelerateFactor;

            _accelerateFactor = _initialAccelerateFactor;
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            if (Data.Length < WindowSize)
            {
                UpdateInternal(bar);

                return 0.0;
            }

            if (!_initialized)
            {
                _initialized = true;
                _accelerateFactor = _initialAccelerateFactor;

                _ascending = Data[-1].ClosePrice > Data[-2].ClosePrice;

                _sar = _ascending ? _lowestPrice : _highestPrice;
            }
            else
            {
                if ((_ascending && _sar > Data[-1].LowestPrice)
                    || (!_ascending && _sar < Data[-1].HighestPrice))
                {
                    // need to turn the trends
                    _ascending = !_ascending;

                    _accelerateFactor = _initialAccelerateFactor;

                    _sar = _ascending ? _lowestPrice : _highestPrice;
                }
                else
                {
                    _sar = _sar + _accelerateFactor * (_ep - _sar);

                    if ((_ascending && bar.HighestPrice > _highestPrice)
                        || (!_ascending && bar.LowestPrice < _lowestPrice))
                    {
                        _accelerateFactor += _accelerateFactorStep;
                        _accelerateFactor = Math.Min(_accelerateFactor, _maxAccelerateFactor);
                    }
                }
            }

            // update data in the middle because SAR(t) is the highest/lowest price of previous N periods 
            // (this period is not included) and EP(t) is the lowest/highest price of latest N periods
            // which include this period.
            UpdateInternal(bar);

            _ep = _ascending ? _highestPrice : _lowestPrice;

            return _sar;
        }

        private void UpdateInternal(StockAnalysis.Share.Bar bar)
        {
            Data.Add(bar);
            _highestPrice = _highestMetric.Update(bar.HighestPrice);
            _lowestPrice = _lowestMetric.Update(bar.LowestPrice);
        }
    }
}
