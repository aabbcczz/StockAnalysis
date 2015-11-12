using System;
using System.Collections.Generic;
using StockAnalysis.Share;

namespace TradingStrategy.Base
{
    public abstract class GeneralTradingStrategyComponentBase : ITradingStrategyComponent
    {
        protected IEvaluationContext Context { get; private set; }

        protected DateTime CurrentPeriod { get; private set; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            return ParameterHelper.GetParameterAttributes(this);
        }

        protected virtual void ValidateParameterValues()
        {
        }

        /// <summary>
        /// opportunity for subclass to register metric to the metric manager in context
        /// </summary>
        protected virtual void RegisterMetric()
        {
        }

        public virtual void Initialize(
            IEvaluationContext context, 
            IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (context == null || parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            ParameterHelper.SetParameterValues(this, parameterValues);

            ValidateParameterValues();

            Context = context;

            // register metric if necessary.
            RegisterMetric();
        }

        public virtual void WarmUp(ITradingObject tradingObject, Bar bar)
        {
            // do nothing
        }

        public virtual void StartPeriod(DateTime time)
        {
            CurrentPeriod = time;
        }

        public virtual void EvaluateSingleObject(ITradingObject tradingObject, Bar bar)
        {
        }

        public virtual void EndPeriod()
        {
            CurrentPeriod = DateTime.MinValue;
        }

        public virtual void Finish()
        {
            // do nothing
        }
    }
}
