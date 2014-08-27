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
        public static IEnumerable<ParameterAttribute> GetParameterAttributes(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            foreach (var property in obj.GetType().GetProperties())
            {
                ParameterAttribute attribute = property.GetCustomAttribute<ParameterAttribute>();

                if (attribute != null)
                {
                    attribute.SetName(property.Name);
                    attribute.SetType(property.PropertyType);
                    attribute.SetTarget(obj, property);

                    yield return attribute;
                }
            }
        }

        public static void SetParameterValues(object obj, IDictionary<ParameterAttribute, object> parameterValues)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (parameterValues == null)
            {
                throw new ArgumentNullException("parameterValues");
            }

            var parameterAttributes = GetParameterAttributes(obj).ToDictionary(p => p.Name);

            // set parameter values specified in parameterValues
            foreach (var kvp in parameterValues)
            {
                if (kvp.Key.TargetObject.GetType() != obj.GetType())
                {
                    continue;
                }

                if (!parameterAttributes.ContainsKey(kvp.Key.Name))
                {
                    throw new InvalidOperationException(string.Format("unknown or duplicated parameter {0}", kvp.Key.Name));
                }

                ParameterAttribute attribute = parameterAttributes[kvp.Key.Name];

                attribute.TargetProperty.SetValue(obj, kvp.Value);

                // remove parameter that has been set value
                parameterAttributes.Remove(kvp.Key.Name);
            }

            // set parameter values to default according to ParameterAttribute
            foreach (var attribute in parameterAttributes.Values)
            {
                attribute.TargetProperty.SetValue(obj, attribute.DefaultValue);
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
