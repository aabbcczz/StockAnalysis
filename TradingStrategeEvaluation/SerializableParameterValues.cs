using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy;

namespace TradingStrategyEvaluation
{
    [Serializable]
    public sealed class SerializableParameterValues
    {
        [Serializable]
        public sealed class NameValuePair
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public NameValuePair[] Parameters { get; set; }

        public void Initialize(IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            Parameters = parameterValues
                .Select(
                    kvp => new NameValuePair
                    {
                        Name = kvp.Key.TargetObject.GetType().Name + "." + kvp.Key.Name,
                        Value = ConvertParameterValueToString(kvp.Value),
                    })
                .OrderBy(nvp => nvp.Name)
                .ToArray();
        }

        private static string ConvertParameterValueToString(object value)
        {
            if (value == null)
            {
                return "(null)";
            }

            if (value is double)
            {
                return ((double)value).ToString("0.000");
            }

            return value.ToString();
        }
    }
}
