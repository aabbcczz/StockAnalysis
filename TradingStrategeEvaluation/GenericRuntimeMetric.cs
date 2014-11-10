using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    internal sealed class GenericRuntimeMetric : IRuntimeMetric
    {
        private readonly MetricExpression _expression;

        public double[] Values 
        {
            get { return _expression.Values; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metricName">the metric definition string, such as "MA[40](BAR.CP)"</param>
        public GenericRuntimeMetric(string metricName)
        {
            if (string.IsNullOrWhiteSpace(metricName))
            {
                throw new ArgumentNullException();
            }

            _expression = MetricEvaluationContext.ParseExpression(metricName);
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            _expression.MultipleOutputUpdate(bar);
        }
    }
}
