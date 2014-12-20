using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategyEvaluation
{
    public sealed class SimpleCapitalManager : ICapitalManager
    {
        public double InitialCapital
        {
            get;
            private set;
        }

        public double CurrentCapital
        {
            get;
            private set;
        }

        public SimpleCapitalManager(double initialCapital, double currentCapital = double.NaN)
        {
            if (initialCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("initalCapital is smaller than 0.0");
            }

            InitialCapital = initialCapital;

            CurrentCapital = double.IsNaN(currentCapital) ? initialCapital : currentCapital;
        }

        private bool AllocateCapital(double requiredCapital, bool allowNegativeCapital)
        {
            if (requiredCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("required capital is smaller than 0.0");
            }

            if (requiredCapital <= CurrentCapital
                || allowNegativeCapital)
            {
                CurrentCapital -= requiredCapital;
                return true;
            }

            return false;
        }

        public bool AllocateCapitalForFirstPosition(double requiredCapital, bool allowNegativeCapital)
        {
            return AllocateCapital(requiredCapital, allowNegativeCapital);
        }

        public bool AllocateCapitalForIncrementalPosition(double requiredCapital, bool allowNegativeCapital)
        {
            return AllocateCapital(requiredCapital, allowNegativeCapital);
        }

        private void FreeCapital(double returnedCapital)
        {
            if (returnedCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("returned capital is smaller than 0.0");
            }

            CurrentCapital += returnedCapital;
        }

        public void FreeCapitalForFirstPosition(double returnedCapital)
        {
            FreeCapital(returnedCapital);
        }

        public void FreeCapitalForIncrementalPosition(double returnedCapital)
        {
            FreeCapital(returnedCapital);
        }
    }
}
