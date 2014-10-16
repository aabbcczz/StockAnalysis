using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public abstract class MetricBasedTradingStrategyComponentBase<T> 
        : GeneralTradingStrategyComponentBase
        where T : IRuntimeMetric
    {
        protected RuntimeMetricManager<T> MetricManager { get; private set; }

        public abstract Func<T> Creator { get; }

        public override void Initialize(
            IEvaluationContext context, 
            IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            if (Creator == null)
            {
                throw new InvalidProgramException("Creator property must not be null");
            }

            MetricManager = new RuntimeMetricManager<T>(Creator, context.GetAllTradingObjects().Count());
        }

        public override void WarmUp(ITradingObject tradingObject, Bar bar)
        {
            MetricManager.Update(tradingObject, bar);
        }

        public override void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
            MetricManager.Update(tradingObject, bar);
        }

        public override void Finish()
        {
            MetricManager = null;
        }
    }
}
