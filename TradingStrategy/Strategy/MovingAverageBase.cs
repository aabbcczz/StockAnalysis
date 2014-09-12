using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy.Strategy
{
    public abstract class MovingAverageBase 
        : MetricBasedTradingStrategyComponentBase<MovingAverageRuntimeMetric>
    {
        [Parameter(5, "短期移动平均周期")]
        public virtual int Short { get; set; }

        [Parameter(20, "长期移动平均周期")]
        public virtual int Long { get; set; }

        public override Func<MovingAverageRuntimeMetric> Creator
        {
            get { return (() => new MovingAverageRuntimeMetric(Short, Long)); }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            Context.Log(string.Format("{2}[Short: {0}, Long: {1}]", Short, Long, this.GetType()));
        }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (Short >= Long)
            {
                throw new ArgumentException("Short parameter value must be smaller than Long parameter value");
            }
        }
    }
}
