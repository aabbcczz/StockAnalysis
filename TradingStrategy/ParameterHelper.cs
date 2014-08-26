using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TradingStrategy
{
    public static class ParameterHelper
    {
        public static IEnumerable<ParameterAttribute> GetParameterAttributes(ITradingStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException("strategy");
            }

            foreach (var property in strategy.GetType().GetProperties())
            {
                ParameterAttribute attribute = property.GetCustomAttribute<ParameterAttribute>();

                if (attribute != null)
                {
                    attribute.SetName(property.Name);
                    attribute.SetType(property.PropertyType);

                    yield return attribute;
                }
            }
        }

        public static void SetParameterValues(ITradingStrategy strategy, IDictionary<string, object> parameterValues)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException("strategy");
            }

            if (parameterValues == null)
            {
                throw new ArgumentNullException("parameterValues");
            }

            var parameterAttributes = GetParameterAttributes(strategy).ToDictionary(p => p.Name);

            // set parameter values specified in parameterValues
            foreach (var kvp in parameterValues)
            {
                if (!parameterAttributes.ContainsKey(kvp.Key))
                {
                    throw new InvalidOperationException(string.Format("unknown or duplicated parameter {0}", kvp.Key));
                }

                ParameterAttribute attribute = parameterAttributes[kvp.Key];
                PropertyInfo property = strategy.GetType().GetProperty(attribute.Name);
                if (property == null)
                {
                    throw new InvalidProgramException(
                        string.Format("There is no property named as {0} in class {1}", attribute.Name, strategy.GetType().FullName));
                }

                property.SetValue(strategy, kvp.Value);

                // remove parameter that has been set value
                parameterAttributes.Remove(kvp.Key);
            }

            // set parameter values to default according to ParameterAttribute
            foreach (var attribute in parameterAttributes.Values)
            {
                PropertyInfo property = strategy.GetType().GetProperty(attribute.Name);
                if (property == null)
                {
                    throw new InvalidProgramException(
                        string.Format("There is no property named as {0} in class {1}", attribute.Name, strategy.GetType().FullName));
                }

                property.SetValue(strategy, attribute.DefaultValue);
            }
        }

        public static bool IsValidValue(ParameterAttribute attribute, string value)
        {
            bool valid = true;
            if (attribute.ParameterType == typeof(int))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    valid = false;
                }
            }
            else if (attribute.ParameterType == typeof(double))
            {
                double result;
                if (!double.TryParse(value, out result))
                {
                    valid = false;
                }
            }
            else if (attribute.ParameterType == typeof(string))
            {
                // nothing to do
            }
            else
            {
                throw new InvalidProgramException();
            }

            return valid;
        }

        public static object ConvertStringToValue(ParameterAttribute attribute, string value)
        {
            object objValue;
            if (attribute.ParameterType == typeof(int))
            {
                objValue = int.Parse(value);
            }
            else if (attribute.ParameterType == typeof(double))
            {
                objValue = double.Parse(value);
            }
            else if (attribute.ParameterType == typeof(string))
            {
                objValue = value;
            }
            else
            {
                throw new InvalidProgramException();
            }

            return objValue;
        }
    }
}
