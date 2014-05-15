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
        public string ShortName { get; set; }

        /// <summary>
        /// Define metric attribute
        /// </summary>
        /// <param name="shortName">short name of metric. e.g. "MA" for MovingAverage</param>
        public MetricAttribute(string shortName)
        {
            ShortName = shortName;
        }
    }
}
