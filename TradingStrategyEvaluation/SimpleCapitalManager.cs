namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;

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

        public bool AllocateCapital(double requiredCapital, bool forFirstPosition, bool allowNegativeCapital)
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

        public void FreeCapital(double returnedCapital, bool forFirstPosition)
        {
            if (returnedCapital < 0.0)
            {
                throw new ArgumentOutOfRangeException("returned capital is smaller than 0.0");
            }

            CurrentCapital += returnedCapital;
        }
    }
}
