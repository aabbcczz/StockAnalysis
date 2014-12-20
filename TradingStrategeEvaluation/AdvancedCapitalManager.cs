using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategyEvaluation
{
    public sealed class AdvancedCapitalManager : ICapitalManager
    {
        private readonly double _initialCapitalForIncrementalPosition;

        private double _currentCapitalForFirstPosition;
        private double _currentCapitalForIncrementalPosition;

        public double InitialCapital
        {
            get;
            private set;
        }

        public double CurrentCapital
        {
            get { return _currentCapitalForFirstPosition + _currentCapitalForIncrementalPosition; }
        }

        public AdvancedCapitalManager(double initialCapital, double proportionOfCapitalForIncrementalPosition)
        {
            if (initialCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("initalCapital is smaller than 0.0");
            }

            if (proportionOfCapitalForIncrementalPosition < 0.0 || proportionOfCapitalForIncrementalPosition > 1.0)
            {
                throw new ArgumentOutOfRangeException("proportionOfCapitalForIncrementalPosition value must be in [0.0..1.0]");
            }

            InitialCapital = initialCapital;

            _currentCapitalForIncrementalPosition = initialCapital * proportionOfCapitalForIncrementalPosition;
            _currentCapitalForFirstPosition = initialCapital - _currentCapitalForIncrementalPosition;

            _initialCapitalForIncrementalPosition = _currentCapitalForIncrementalPosition;
        }

        public bool AllocateCapitalForFirstPosition(double requiredCapital, bool allowNegativeCapital)
        {
            if (requiredCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("required capital is smaller than 0.0");
            }

            if (requiredCapital <= _currentCapitalForFirstPosition
                || allowNegativeCapital)
            {
                _currentCapitalForFirstPosition -= requiredCapital;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AllocateCapitalForIncrementalPosition(double requiredCapital, bool allowNegativeCapital)
        {
            if (requiredCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("required capital is smaller than 0.0");
            }

            if (requiredCapital < _currentCapitalForIncrementalPosition)
            {
                _currentCapitalForIncrementalPosition -= requiredCapital;
                return true;
            }
            else if (requiredCapital <= CurrentCapital
                || allowNegativeCapital)
            {
                _currentCapitalForFirstPosition -= requiredCapital - _currentCapitalForIncrementalPosition;
                _currentCapitalForIncrementalPosition = 0.0;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FreeCapitalForFirstPosition(double returnedCapital)
        {
            if (returnedCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("returned capital is smaller than 0.0");
            }

            _currentCapitalForFirstPosition += returnedCapital;
        }

        public void FreeCapitalForIncrementalPosition(double returnedCapital)
        {
            if (returnedCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("returned capital is smaller than 0.0");
            }

            _currentCapitalForIncrementalPosition += returnedCapital;

            if (_currentCapitalForIncrementalPosition > _initialCapitalForIncrementalPosition)
            {
                // move back to capital for first position.
                _currentCapitalForFirstPosition += _currentCapitalForIncrementalPosition - _initialCapitalForIncrementalPosition;
                _currentCapitalForIncrementalPosition = _initialCapitalForIncrementalPosition;
            }
        }
    }
}
