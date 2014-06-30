using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("KST")]
    public sealed class KnowSureThings : SingleOutputRawInputSerialMetric
    {
        private RateOfChange _roc1;
        private RateOfChange _roc2;
        private RateOfChange _roc3;
        private RateOfChange _roc4;

        public KnowSureThings(int windowSize1, int windowSize2, int windowSize3, int windowSize4)
            : base(1)
        {
            if (windowSize1 <= 0 || windowSize2 <= 0 || windowSize3 <= 0 || windowSize4 <= 0)
            {
                throw new ArgumentOutOfRangeException("windowSize");
            }

            _roc1 = new RateOfChange(windowSize1);
            _roc2 = new RateOfChange(windowSize2);
            _roc3 = new RateOfChange(windowSize3);
            _roc4 = new RateOfChange(windowSize4);
        }

        public override double Update(double dataPoint)
        {
            double roc1 = _roc1.Update(dataPoint);
            double roc2 = _roc2.Update(dataPoint);
            double roc3 = _roc3.Update(dataPoint);
            double roc4 = _roc4.Update(dataPoint);

            return roc1 + 2 * roc2 + 3 * roc3 + 4 * roc4 / (1 + 2 + 3 + 4);
        } 
    }
}
