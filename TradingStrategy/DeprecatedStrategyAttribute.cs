using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TradingStrategy
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DeprecatedStrategyAttribute : Attribute
    {
        public DeprecatedStrategyAttribute()
        {
        }
    }
}
