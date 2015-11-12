using System;
using System.Collections.Generic;
using System.Linq;
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
                var attribute = property.GetCustomAttribute<ParameterAttribute>();

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

                var attribute = parameterAttributes[kvp.Key.Name];

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

        public static bool TryParse(ParameterAttribute attribute, string value, out object obj)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            return TryParse(attribute.ParameterType, value, out obj);
        }

        public static bool TryParse(Type type, string value, out object obj)
        {
            obj = null;

            if (type == null)
            {
                throw new ArgumentNullException();
            }

            var isValid = true;
            if (type == typeof(int))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    isValid = false;
                }
                else
                {
                    obj = result;
                }
            }
            else if (type == typeof(double))
            {
                double result;
                if (!double.TryParse(value, out result))
                {
                    isValid = false;
                }
                else
                {
                    obj = result;
                }
            }
            else if (type == typeof(string))
            {
                // nothing to do
                obj = value;
            }
            else if (type == typeof(bool))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    isValid = false;
                }
                else
                {
                    obj = result;
                }
            }
            else if (type.IsEnum)
            {
                try
                {
                    obj = Enum.Parse(type, value, true);
                    isValid = true;
                }
                catch
                {
                    obj = null;
                    isValid = false;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            return isValid;
        }

        public static object Parse(ParameterAttribute attribute, string value)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            return Parse(attribute.ParameterType, value);
        }

        public static object Parse(Type type, string value)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }

            object obj;

            if (!TryParse(type, value, out obj))
            {
                throw new InvalidOperationException(
                    string.Format(@"failed to convert ""{0}"" to type ""{1}""", value, type.FullName));
            }

            return obj;
        }
    }
}
