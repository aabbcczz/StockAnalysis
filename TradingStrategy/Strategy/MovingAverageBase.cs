using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public class MovingAverageBase : MetricBasedTradingStrategyComponentBase<MovingAverageRuntimeMetric>, ITradingStrategyComponent
    {
        protected IEvaluationContext _context;

        [Parameter(5, "短期移动平均周期")]
        public int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public int Long { get; set; }

        public virtual string Name { get { return string.Empty; } }

        public virtual string Description { get { return string.Empty; } }

        public virtual IEnumerable<ParameterAttribute> GetParameterDefinitions()
        {
            return ParameterHelper.GetParameterAttributes(this);
        }

        public virtual void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (context == null || parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            ParameterHelper.SetParameterValues(this, parameterValues);

            if (Short >= Long)
            {
                throw new ArgumentException("Short parameter value must be smaller than Long parameter value");
            }

            base.Initialize(() => new MovingAverageRuntimeMetric(Short, Long));

            _context = context;

            _context.Log(string.Format("{2}[Short: {0}, Long: {1}]", Short, Long, this.GetType()));
        }

        public new virtual void WarmUp(ITradingObject tradingObject, StockAnalysis.Share.Bar bar)
        {
            base.WarmUp(tradingObject, bar);
        }


        public virtual void StartPeriod(DateTime time)
        {
            // nothing 
        }

        public new virtual void Evaluate(ITradingObject tradingObject, Bar bar)
        {
            if (bar.Invalid())
            {
                return;
            }

            base.Evaluate(tradingObject, bar);
        }

        public virtual void EndPeriod()
        {
            // nothing
        }

        public new virtual void Finish()
        {
            base.Finish();
        }
    }
}
