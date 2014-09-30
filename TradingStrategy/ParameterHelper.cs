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
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            return IsValidValue(attribute.ParameterType, value);
        }

        public static bool IsValidValue(Type type, string value)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }

            bool valid = true;
            if (type == typeof(int))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    valid = false;
                }
            }
            else if (type == typeof(double))
            {
                double result;
                if (!double.TryParse(value, out result))
                {
                    valid = false;
                }
            }
            else if (type == typeof(string))
            {
                // nothing to do
            }
            else
            {
                throw new InvalidOperationException();
            }

            return valid;
        }

        public static object ConvertStringToValue(ParameterAttribute attribute, string value)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            return ConvertStringToValue(attribute.ParameterType, value);
        }

        public static object ConvertStringToValue(Type type, string value)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }

            object objValue;
            if (type == typeof(int))
            {
                objValue = int.Parse(value);
            }
            else if (type == typeof(double))
            {
                objValue = double.Parse(value);
            }
            else if (type == typeof(string))
            {
                objValue = value;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return objValue;
        }
    }
}
