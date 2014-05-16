using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MetricAttribute : Attribute
    {
        private Dictionary<string, int> _subfields = new Dictionary<string, int>();
        public string ShortName { get; set; }
        public IDictionary<string, int> NameToFieldIndexMap { get { return _subfields; } }
        /// <summary>
        /// Define metric attribute
        /// </summary>
        /// <param name="shortName">short name of metric. e.g. "MA" for MovingAverage</param>
        /// <param name="subfields">subfields' description. Each subfield is separated by ',', e.g. "DIF,DEA" for MACD</param>
        public MetricAttribute(string shortName, string subfields = "V")
        {
            ShortName = shortName;

            if (!string.IsNullOrWhiteSpace(subfields))
            {
                string[] fields = subfields.Split(new char[] { ',' });
                for (int i = 0; i < fields.Length; ++i)
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
