using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public class Metric
    {
        private MetricType _type;
        private object[] _parameters;

        public MetricType type { get { return _type; } }
        public object[] Parameters { get { return _parameters; } }

        
        public Metric(MetricType type, params object[] parameters)
        {
            _type = type;
            _parameters = parameters;
        }
    }
}
