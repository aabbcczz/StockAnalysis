using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    public static class MetricEvaluator
    {
        public static Dictionary<string, MetricAttribute> NameToMetricAttributeMap = new Dictionary<string, MetricAttribute>();

        public static Dictionary<string, Type> NameToMetricMap = new Dictionary<string, Type>();

        static MetricEvaluator()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var metrics = assembly.GetTypes()
                .Where(type => type.IsClass 
                    && typeof(IMetric).IsAssignableFrom(type));

            foreach (var metric in metrics)
            {
                MetricAttribute attribute = metric.GetCustomAttribute<MetricAttribute>();

                if (attribute == null)
                {
                    throw new InvalidProgramException(string.Format("Metric class {0} has not been associated with MetricAttribute", metric.Name));
                }

                if (NameToMetricAttributeMap.ContainsKey(attribute.ShortName))
                {
                    throw new InvalidProgramException(
                        string.Format(
                            "Short name {0} has been defined for class {1}", 
                            attribute.ShortName, 
                            NameToMetricMap[attribute.ShortName].Name));
                }

                NameToMetricAttributeMap.Add(attribute.ShortName, attribute);

                NameToMetricMap.Add(attribute.ShortName, metric);
            }
        }

        /// <summary>
        /// Evaluate a metric by name and parameter in expression
        /// </summary>
        /// <param name="expression">
        /// Expression to be evaluated.
        /// Expression is like M1[P1,P2,...](M2[P3,P4,...](...)), where M1/M2 is the short name of metric,
        /// P1,P2,P3,P4 is the corresponding parameter for the metric. operator() means the M2 output is the input
        /// of M1.
        /// </param>
        /// <param name="data">input data for evaluation</param>
        public static IEnumerable<double> Evaluate(string expression, IEnumerable<StockTransactionSummary> data)
        {
            return ParseExpression(expression).Evaluate(data);
        }

        public static IEnumerable<double> Evaluate(string expression, IEnumerable<double> data)
        {

            return ParseExpression(expression).Evaluate(data);
        }

        private static MetricExpression ParseExpression(string expression)
        {
            string errorMessage;
            MetricExpression metricExpression =
                new MetricExpressionParser().Parse(expression, out errorMessage);

            if (metricExpression == null)
            {
                throw new InvalidOperationException(
                    string.Format("Parse expression {0} failed. \nError message: {1}", expression, errorMessage));
            }

            return metricExpression;
        }
    }
}
