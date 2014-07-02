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

        // start a new period. This function is called each time when a new period is started. 
        // After that, system will call Evaluate function for each trading object.
        // And then call EndPeriod to finish evaluating this period.
        // The value of parameter 'time' will be in ascending order for each call of this function.
        public void StartPeriod(DateTime time);

        public void Evaluate(ITradingObject tradingObject, Bar bar);

        public IEnumerable<Instruction> EndPeriod();

    }
}
