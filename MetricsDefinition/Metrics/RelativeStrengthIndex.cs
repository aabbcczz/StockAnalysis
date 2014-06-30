using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("RSI")]
    public sealed class RelativeStrengthIndex : SingleOutputRawInputSerialMetric
    {
        private MovingSum _msUc;
        private MovingSum _msDc;
        private double _prevData;
        private bool _firstData = true;
        public RelativeStrengthIndex(int windowSize)
            : base(1)
        {
            _msUc = new MovingSum(windowSize);
            _msDc = new MovingSum(windowSize);
        }

        public override double Update(double dataPoint)
        {
            double uc = _firstData
                ? 0.0
                : Math.Max(0.0, dataPoint - _prevData);

            double dc = _firstData
                ? 0.0
                : Math.Max(0.0, _prevData - dataPoint);

            double msuc = _msUc.Update(uc);
            double msdc = _msDc.Update(dc);

            // update status
            _prevData = dataPoint;
            _firstData = false;

            return (msuc + msdc == 0.0) ? 0.0 : msuc / (msuc + msdc) * 100.0;
        }
    }
}
