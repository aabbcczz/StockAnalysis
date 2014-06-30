using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class BarInputSerialMetric : SerialMetric
    {
        private CirculatedArray<Bar> _data;

        internal CirculatedArray<Bar> Data
        {
            get { return _data; }
        }

        public BarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            _data = new CirculatedArray<Bar>(windowSize);
        }
    }
}
