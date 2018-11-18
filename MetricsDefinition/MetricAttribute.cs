namespace StockAnalysis.MetricsDefinition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MetricAttribute : Attribute
    {
        private readonly Dictionary<string, int> _subfields = new Dictionary<string, int>();
        public string[] ShortNames { get; private set; }
        public IDictionary<string, int> NameToFieldIndexMap { get { return _subfields; } }

        /// <summary>
        /// Define metric attribute
        /// </summary>
        /// <param name="shortNames">short names of metric. e.g. "EMA,EXPMA" for ExponentialMovingAverage. Multiple short names can be separated by ','</param>
        /// <param name="subfields">subfields' description. Each subfield is separated by ',', e.g. "DIF,DEA" for MACD</param>
        public MetricAttribute(string shortNames, string subfields = "V")
        {
            ShortNames = shortNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();

            if (!string.IsNullOrWhiteSpace(subfields))
            {
                var fields = subfields.Split(new[] { ',' });
                for (var i = 0; i < fields.Length; ++i)
                {
                    if (string.IsNullOrWhiteSpace(fields[i]))
                    {
                        throw new ArgumentException("no empty subfield is allowed if other subfields have already been defined");
                    }

                    if (_subfields.ContainsKey(fields[i]))
                    {
                        throw new ArgumentException("subfield " + fields[i] + " is defined duplicately");
                    }

                    _subfields.Add(fields[i], i);
                }
            }
        }
    }
}
