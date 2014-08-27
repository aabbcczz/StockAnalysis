using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TradingStrategy
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ParameterAttribute : Attribute 
    {
        public string Name { get; private set; }

        public Type ParameterType { get; private set; }

        public string Description { get; set; }

        public object DefaultValue { get; set; }

        public object TargetObject { get; private set; }

        public PropertyInfo TargetProperty {get; private set;}

        public ParameterAttribute(object defaultValue = null, string description = "")
        {
            DefaultValue = defaultValue;
            Description = description;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
        }

        public void SetType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!ParameterAttribute.IsSupportedType(type))
            {
                throw new ArgumentException(string.Format("unsupported type {0}", type.FullName));
            }

            if (type == ParameterType)
            {
                return;
            }

            ParameterType = type;

            if (DefaultValue.GetType() != type)
            {
                throw new InvalidProgramException("At least one value in Default/Min/Max/Step is not expected type");
            }
        }
        
        public void SetTarget(object obj, PropertyInfo property)
        {
            if (obj == null || property == null)
            {
                throw new ArgumentNullException();
            }

            TargetObject = obj;
            TargetProperty = property;
        }

        public static bool IsSupportedType(Type type)
        {
            return (type == typeof(int)
                || type == typeof(double)
                || type == typeof(string));
        }
    }
}
