using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public interface ITradingStrategy
    {
        public TradingSettings GetTradingSettings();

        public double GetInitialCapital();

        // initialize the strategy with evaluation context
        public void Initialize(ITradingStrategyEvaluationContext context);

        // Warm up the strategy. this function will be called many times to traverse all warming up data
        public void WarmUp(ITradingObject tradingObject, Bar bar);

        // finish evaluation, chance of cleaning up resources and do some other works
        public void Finish();

        // Call sequence for one period:
        // StartPeriod()
        // NotifyTransactionStatus() for the transactions submitted in previous period
        //      but that need to be executed in this period.
        // Evaluate(). this function will be called for each valid trading object.
        // GetInstructions() to get all required transactions from the bar in this period.
        // NotifyTransactionStatus() for the transactions submitted AND executed in this period. Some transactions will be
        //      executed in the next period and the status will be updated in the next UpdateTransactionStatus() call in the next period.
        // EndPeriod()

        // start a new period.
        // The value of parameter 'time' will be in ascending order for each call of this function.
        public void StartPeriod(DateTime time);

        public void (Transaction transaction);

        public void Evaluate(ITradingObject tradingObject, Bar bar);

        public IEnumerable<Instruction> GetInstructions();

        public void EndPeriod();

    }
}
