namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        public void Initialize(IDictionary<Tuple<int, ParameterAttribute>, object> parameterValues)
        {
            if (parameterValues == null)
            {
                throw new ArgumentNullException();
            }

            Parameters = parameterValues
                .Select(
                    kvp => new NameValuePair
                    {
                        Name = kvp.Key.Item1.ToString() + "_" + kvp.Key.Item2.TargetObject.GetType().Name + "." + kvp.Key.Item2.Name,
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
