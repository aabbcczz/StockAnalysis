using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy;
namespace EvaluatorCmdClient
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

            var s = value as string;
            if (s != null)
            {
                return s;
            }

            if (value is int)
            {
                return value.ToString();
            }

            if (value is double)
            {
                return ((double)value).ToString("0.000");
            }

            throw new InvalidOperationException(
                string.Format("unsupported type {0}", value.GetType().FullName));
        }
    }
}
