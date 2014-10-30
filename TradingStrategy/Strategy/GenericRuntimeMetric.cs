using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetricsDefinition;

namespace TradingStrategy.Strategy
{
    public sealed class GenericRuntimeMetric : IRuntimeMetric
    {
        private readonly MetricExpression[] _expressions;
        private readonly bool _needPreviousData;

        public string[] MetricNames { get; private set; }

        public string[][] MetricFieldNames { get; private set; }

        public double[][] LatestData { get; private set; }

        public double[][] PreviousData { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metricDefinitions">
        /// the metric definition string, each metric is separated by ";". 
        /// Metric definition is like "MA[40](BAR.CP)"
        /// </param>
        public GenericRuntimeMetric(string metricDefinitions, bool needPreviousData = false)
        {
            if (string.IsNullOrWhiteSpace(metricDefinitions))
            {
                throw new ArgumentNullException();
            }

            MetricNames = metricDefinitions
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(me => !string.IsNullOrWhiteSpace(me))
                .ToArray();

            _expressions = MetricNames
                .Select(MetricEvaluationContext.ParseExpression)
                .ToArray();

            _needPreviousData = needPreviousData;

            MetricFieldNames = _expressions.Select(me => me.FieldNames).ToArray();
        }

        public void Update(StockAnalysis.Share.Bar bar)
        {
            if (_needPreviousData)
            {
                PreviousData = LatestData;
            }

            LatestData = _expressions.Select(me => me.MultipleOutputUpdate(bar)).ToArray();
        }
    }
}
