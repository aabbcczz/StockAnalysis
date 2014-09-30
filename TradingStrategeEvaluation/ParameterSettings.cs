using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;

namespace TradingStrategyEvaluation
{
    [Serializable]
    public sealed class ParameterSettings
    {
        private object[] _parsedValues = null;

        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueType { get; set; }
        public object DefaultValue { get; set; }

        /// <summary>
        /// Values of parameter. it supports serval formats:
        /// 1. single value string. 
        /// 2. multiple values separated by ";" (value type is int or double) or by "(;)" (value type is string)
        /// 3. if value type is int or double, value string like "1/10/1" represents "start/end/step", so 
        /// it means values "1;2;3;4;5;6;7;8;9;10"
        /// </summary>
        public string Values { get; set; }

        [NonSerialized]
        public object[] ParsedValues { get { return _parsedValues; } }

        public static ParameterSettings GenerateExampleSettings(ParameterAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            ParameterSettings settings = new ParameterSettings();

            settings.Name = attribute.Name;
            settings.Description = attribute.Description;
            settings.ValueType = attribute.ParameterType.FullName;
            settings.DefaultValue = attribute.DefaultValue;
            settings.Values = "1;2;1.0/10.0/1.0";

            return settings;
        }

        public object[] ParseValueString()
        {
            
        }
    }
}
