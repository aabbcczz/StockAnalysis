using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        // AfterEvaluation(). this function provide an opportunity for the strategy to create instructions 
        //      after knowing all required information.
        // GetInstructions() to get all required transactions from the bar in this period.
        // NotifyTransactionStatus() for the transactions submitted AND executed in this period. Some transactions will be
        //      executed in the next period and the status will be updated in the next UpdateTransactionStatus() call in the next period.
        // EndPeriod()
        void AfterEvaluation();

        void NotifyTransactionStatus(Transaction transaction);

        IEnumerable<Instruction> RetrieveInstructions();
    }
}
