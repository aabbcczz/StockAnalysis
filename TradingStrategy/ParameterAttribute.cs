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
        public bool Required { get; set; }

        public string Name { get; private set; }

        public Type ParameterType { get; private set; }

        public string Description { get; set; }

        public object DefaultValue { get; set; }

        public object Min { get; set; }

        public object Max { get; set; }

        public object Step { get; set; }

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

            if (DefaultValue.GetType() != type
                || Min.GetType() != type
                || Max.GetType() != type
                || Step.GetType() != type)
            {
                throw new InvalidProgramException("At least one value in Default/Min/Max/Step is not expected type");
            }

            if (type == typeof(int))
            {
                int defaultValue = (int)DefaultValue;
                int min = (int)Min;
                int max = (int)Max;
                int step = (int)1;


                if (min > max)
                {
                    throw new InvalidProgramException("Min value is greater than Max value");
                }

                if (step <= 0)
                {
                    throw new InvalidProgramException("Step <= 0");
                }

                if (defaultValue < min || defaultValue > max)
                {
                    throw new InvalidProgramException("Default value is not inside [Min, Max]");
                }
            }
            else if (type == typeof(double))
            {
                double defaultValue = (double)DefaultValue;
                double min = (double)Min;
                double max = (double)Max;
                double step = 1.0;

                if (min > max)
                {
                    throw new InvalidProgramException("Min value is greater than Max value");
                }

                if (step <= 0.0)
                {
                    throw new InvalidProgramException("Step <= 0.0");
                }

                if (defaultValue < min || defaultValue > max)
                {
                    throw new InvalidProgramException("Default value is not inside [Min, Max]");
                }
            }
        }
        
        public static bool IsSupportedType(Type type)
        {
            return (type == typeof(int)
                || type == typeof(double)
                || type == typeof(string));
        }
    }
}
