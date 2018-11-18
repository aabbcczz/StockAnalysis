using System;
using System.Collections.Generic;
using StockAnalysis.Common.Data;

namespace StockAnalysis.TradingStrategy
{
    public interface ITradingStrategyComponent
    {
        string Name { get; }

        string Description { get; }

        IEnumerable<ParameterAttribute> GetParameterDefinitions();

        // initialize the strategy component with evaluation context and parameters.
        void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues);

        // Warm up the strategy component. this function will be called many times to traverse all warming up data
        // this function could be called in parallel if SupportParallelization is true
        void WarmUp(ITradingObject tradingObject, Bar bar);

        // Call sequence for one period:
        // StartPeriod()
        // Evaluate() for all bars
        // EndPeriod()

        // start a new period.
        // The value of parameter 'time' will be in ascending order for each call of this function.
        void StartPeriod(DateTime time);

        // Evaluate bar for a given trading object.
        void EvaluateSingleObject(ITradingObject tradingObject, Bar bar);

        void EndPeriod();

        // finish evaluation, chance of cleaning up resources and do some other works
        void Finish();
    }
}
