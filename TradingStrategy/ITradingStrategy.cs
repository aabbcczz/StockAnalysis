using System.Collections.Generic;
using StockAnalysis.Share;

namespace TradingStrategy
{
    public interface ITradingStrategy : ITradingStrategyComponent
    {
        // Call sequence for one period:
        // StartPeriod()
        // NotifyTransactionStatus() for the transactions submitted in previous period
        //      but that need to be executed in this period.
        // Evaluate(). this function will be called for each valid trading object.
        // GetInstructions() to get all required transactions from the bar in this period.
        // EstimateStoplossAndSizeForNewPositions() to get the stop loss and size of all new positions
        // NotifyTransactionStatus() for the transactions submitted AND executed in this period. Some transactions will be
        //      executed in the next period and the status will be updated in the next UpdateTransactionStatus() call in the next period.
        // EndPeriod()
        void Evaluate(ITradingObject[] tradingObjects, Bar[] bars);

        void EstimateStoplossAndSizeForNewPosition(Instruction instruction, double price, int totalNumberOfObjectsToBeEstimated);

        void NotifyTransactionStatus(Transaction transaction);

        IEnumerable<Instruction> RetrieveInstructions();
    }
}
