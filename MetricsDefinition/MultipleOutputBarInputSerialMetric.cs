using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class MultipleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        public MultipleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double[] Update(Bar bar);
    }
}
