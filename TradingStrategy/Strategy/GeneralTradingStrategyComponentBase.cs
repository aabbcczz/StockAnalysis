using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public abstract class GeneralTradingStrategyComponentBase : ITradingStrategyComponent
    {
        protected IEvaluationContext Context { get; private set; }

        protected DateTime Period { get; private set; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            return ParameterHelper.GetParameterAttributes(this);
        }

        protected virtual void ValidateParameterValues()
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
        }

        public virtual void WarmUp(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            // do nothing
        }

        public virtual void StartPeriod(DateTime time)
        {
            Period = time;
        }

        public virtual void EvaluateSingleObject(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            // do nothing
        }

        public virtual void EndPeriod()
        {
            Period = DateTime.MinValue;
        }

        public virtual void Finish()
        {
            // do nothing
        }
    }
}
