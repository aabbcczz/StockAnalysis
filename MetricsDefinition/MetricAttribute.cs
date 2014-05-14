using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class MetricAttribute : Attribute
    {
        public class ParameterDescription
        {
            public string ParameterName { get; set; }

            public Type ParameterType { get; set; }
        }

        private List<ParameterDescription> _parameterDescriptions = new List<ParameterDescription>();

        public IEnumerable<ParameterDescription> ParameterDescriptions { get { return _parameterDescriptions; } }

        public string ShortName { get; set; }

        /// <summary>
        /// Define metric attribute
        /// </summary>
        /// <param name="shortName">short name of metric. e.g. "MA" for MovingAverage</param>
        /// <param name="parameterDescription">the description of parameters.
        /// The description consists of one or more parts seperated by ';', 
        /// and each part describes one parameter as "name:type", e.g. "days:int". 
        /// Type should be standard C# type name that can be found in the assembly 
        /// </param>
        public MetricAttribute(string shortName, string parameterDescription = "")
        {
            ShortName = shortName;

            if (!string.IsNullOrWhiteSpace(parameterDescription))
            {
                var descriptions = parameterDescription.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

                foreach (var description in descriptions)
                {
                    string[] fields = description.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (fields.Length != 2)
                    {
                        throw new ArgumentException("Parameter description \"" + description + "\" is invalid");
                    }

                    // check if field[1] is valid type.
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    Type parameterType = null;

                    foreach (var assembly in assemblies)
                    {
                        parameterType = assembly.GetType(fields[1]);
                        if (parameterType != null)
                        {
                            break;
                        }
                    }

                    if (parameterType == null)
                    {
                        throw new ArgumentException("Invalid parameter type \"" + fields[1] + "\"");
                    }

                    _parameterDescriptions.Add(new ParameterDescription { ParameterName = fields[0], ParameterType = parameterType });
                }
            }
        }
    }
}
