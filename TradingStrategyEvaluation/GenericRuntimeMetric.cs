namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;
    using StockAnalysis.Common.Data;
    using StockAnalysis.MetricsDefinition;

    public sealed class GenericRuntimeMetric : IRuntimeMetric
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

        public void Update(Bar bar)
        {
            _expression.MultipleOutputUpdate(bar);
        }
    }
}
