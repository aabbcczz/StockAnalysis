using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using TradingStrategy;

namespace TradingStrategyEvaluation
{
    [Serializable]
    public sealed class ParameterSettings
    {
        public const string MultipleValueSeparator = ";";
        public const string MultipleStringValueSeparator = "(;)";
        public const string LoopSeparator = "/";

        private List<object> _parsedValues = null;

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

        public IEnumerable<object> GetParsedValues()
        {
            if (_parsedValues == null)
            {
                ParseValues();
            }

            return _parsedValues; 
        } 

        public static ParameterSettings GenerateExampleSettings(ParameterAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            ParameterSettings settings = new ParameterSettings();

            settings.Name = attribute.Name;
            settings.Description = attribute.Description;
            settings.ValueType = attribute.ParameterType.AssemblyQualifiedName;
            settings.DefaultValue = attribute.DefaultValue;
            settings.Values = "1;2 or 1.0/10.0/1.0 or abcd(;)efgh";

            return settings;
        }

        private void ParseValues()
        {
            if (_parsedValues != null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Values))
            {
                _parsedValues = new List<object>();
                return;
            }

            try
            {
                Type valueType = Type.GetType(ValueType);

                if (valueType == typeof(int)
                    || valueType == typeof(double))
                {
                    var substrings = Values.Split(
                        new string[] { MultipleValueSeparator }, 
                        StringSplitOptions.None);

                    if (substrings.Length > 1
                        || Values.IndexOf(LoopSeparator) < 0) // not loop
                    {
                        _parsedValues = substrings
                            .Select(s => ParameterHelper.ConvertStringToValue(valueType, s))
                            .ToList();
                    }
                    else
                    {
                        // loop
                        var fields = Values.Split(
                            new string[] { LoopSeparator }, 
                            StringSplitOptions.None); 
        
                        if (fields.Length != 3)
                        {
                            throw new InvalidOperationException(
                                string.Format("{0} is not correct loop format", Values));
                        }

                        _parsedValues = GenerateValuesForLoop(valueType, fields[0], fields[1], fields[2]).ToList();
                    }
                }
                else if (valueType == typeof(string))
                {
                    var substrings = Values.Split(
                        new string[] { MultipleStringValueSeparator }, 
                        StringSplitOptions.None);

                    _parsedValues = substrings
                        .Select(s => ParameterHelper.ConvertStringToValue(valueType, s))
                        .ToList();
                }
                else
                {
                    throw new InvalidOperationException("unsupported value type");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Failed to parse parameter value {0} for parameter {1}. Inner exception: {2}",
                        Values,
                        Name,
                        ex));
            }
        }

        private IEnumerable<object> GenerateValuesForLoop(Type type, string start, string end, string step)
        {
            object startObj = ParameterHelper.ConvertStringToValue(type, start);
            object endObj = ParameterHelper.ConvertStringToValue(type, end);
            object stepObj = ParameterHelper.ConvertStringToValue(type, step);

            if (type == typeof(int))
            {
                return GenerateValuesForIntLoop((int)startObj, (int)endObj, (int)stepObj);
            }
            else if (type == typeof(double))
            {
                return GenerateValuesForDoubleLoop((double)startObj, (double)endObj, (double)stepObj);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private IEnumerable<object> GenerateValuesForIntLoop(int start, int end, int step)
        {
            if (start > end || step <= 0)
            {
                throw new ArgumentException("start > end or step <= 0");
            }

            for (int i = start; i <= end; i += step)
            {
                yield return i;
            }
        }

        private IEnumerable<object> GenerateValuesForDoubleLoop(double start, double end, double step)
        {
            if (start > end || step <= 0.0)
            {
                throw new ArgumentException("start > end or step <= 0.0");
            }

            for (double i = start; i <= end; i += step)
            {
                yield return i;
            }
        }
    }
}
