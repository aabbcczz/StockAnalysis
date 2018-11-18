namespace StockAnalysis.TradingStrategy.Evaluation
{
    public interface ICapitalManager
    {
        double InitialCapital { get; }

        double CurrentCapital { get; }

        bool AllocateCapital(double requiredCapital, bool forFirstPosition, bool allowNegativeCapital);

        void FreeCapital(double returnedCapital, bool forFirstPosition);
    }
}
