using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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
                    && (typeof(IGeneralMetric).IsAssignableFrom(type)
                        || typeof(IStockDailySummaryMetric).IsAssignableFrom(type)));

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


        public static void Evaluate(string expression)
        {

        }
    }
}
