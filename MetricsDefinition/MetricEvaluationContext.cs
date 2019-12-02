namespace StockAnalysis.MetricsDefinition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class MetricEvaluationContext
    {
        private static readonly Dictionary<string, MetricAttribute> NameToMetricAttributeMap = new Dictionary<string, MetricAttribute>();

        public static readonly Dictionary<string, Type> NameToMetricMap = new Dictionary<string, Type>();

        static MetricEvaluationContext()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var metrics = assembly.GetTypes()
                .Where(type => type.IsClass 
                    && typeof(SerialMetric).IsAssignableFrom(type));

            foreach (var metric in metrics)
            {
                if (metric == typeof(SerialMetric)
                    || metric == typeof(BarInputSerialMetric)
                    || metric == typeof(RawInputSerialMetric)
                    || metric == typeof(SingleOutputBarInputSerialMetric)
                    || metric == typeof(MultipleOutputBarInputSerialMetric)
                    || metric == typeof(SingleOutputRawInputSerialMetric)
                    || metric == typeof(MultipleOutputRawInputSerialMetric))

                {
                    continue;
                }

                if (!typeof(SingleOutputBarInputSerialMetric).IsAssignableFrom(metric)
                    && !typeof(SingleOutputRawInputSerialMetric).IsAssignableFrom(metric)
                    && !typeof(MultipleOutputBarInputSerialMetric).IsAssignableFrom(metric)
                    && !typeof(MultipleOutputRawInputSerialMetric).IsAssignableFrom(metric))
                {
                    throw new InvalidProgramException(
                        string.Format(
                            "Metric {0} is not inherited from class {1}, {2}, {3}, or {4}",
                            metric,
                            typeof(SingleOutputBarInputSerialMetric).Name,
                            typeof(SingleOutputRawInputSerialMetric).Name,
                            typeof(MultipleOutputBarInputSerialMetric).Name,
                            typeof(MultipleOutputRawInputSerialMetric).Name));
                }

                var attribute = metric.GetCustomAttribute<MetricAttribute>();

                if (attribute == null)
                {
                    throw new InvalidProgramException(
                        string.Format(
                            "Metric class {0} has not been associated with MetricAttribute", 
                            metric.Name));
                }
                
                // validate if metric's attribute consistent with metric class
                if (typeof(SingleOutputBarInputSerialMetric).IsAssignableFrom(metric)
                    || typeof(SingleOutputRawInputSerialMetric).IsAssignableFrom(metric))
                {
                    if (attribute.NameToFieldIndexMap.Count != 1)
                    {
                        throw new InvalidProgramException(
                            string.Format(
                                "Metric class {0} should have only single output, but its attribute declared {1} outputs",
                                metric.Name,
                                attribute.NameToFieldIndexMap.Count));
                    }
                }
                else if (typeof(MultipleOutputBarInputSerialMetric).IsAssignableFrom(metric)
                    || typeof(MultipleOutputRawInputSerialMetric).IsAssignableFrom(metric))
                {
                    if (attribute.NameToFieldIndexMap.Count <= 1)
                    {
                        throw new InvalidProgramException(
                            string.Format(
                                "Metric class {0} should have multiple output, but its attribute declared {1} output",
                                metric.Name,
                                attribute.NameToFieldIndexMap.Count));
                    }
                }
                else
                {
                    // should never be here because above code has checked if metric is inherited 
                    // from the expected base classes
                    throw new InvalidProgramException("Run to unexpected code");
                }

                // validate metric names
                foreach (var name in attribute.ShortNames)
                {
                    if (NameToMetricAttributeMap.ContainsKey(name))
                    {
                        throw new InvalidProgramException(
                            string.Format(
                                "Short name {0} has been defined for class {1}",
                                name,
                                NameToMetricMap[name].Name));
                    }

                    NameToMetricAttributeMap.Add(name, attribute);

                    NameToMetricMap.Add(name, metric);
                }
            }
        }

        public static MetricExpression ParseExpression(string expression)
        {
            var parser = new MetricExpressionParser();
            var metricExpression = parser.Parse(expression);

            if (metricExpression == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Parse expression {0} failed. \nError message: {1}", 
                        expression, 
                        parser.LastErrorMessage));
            }

            return metricExpression;
        }
    }
}
