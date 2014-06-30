using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public abstract class RawInputSerialMetric : SerialMetric
    {
        private CirculatedArray<double> _data;

        internal CirculatedArray<double> Data
        {
            get { return _data; }
        }

        public RawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            _data = new CirculatedArray<double>(windowSize);
        }
    }
}
