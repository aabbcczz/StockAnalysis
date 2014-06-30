using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("VR")]
    public sealed class VolumeRatio : SingleOutputBarInputSerialMetric
    {
        private double _prevClosePrice = 0.0;
        private bool _firstBar = true;

        private MovingSum _msPv;
        private MovingSum _msNv;
        private MovingSum _msZv;

        public VolumeRatio(int windowSize)
            : base(1)
        {
            _msPv = new MovingSum(windowSize);
            _msNv = new MovingSum(windowSize);
            _msZv = new MovingSum(windowSize);
        }

        public override double Update(StockAnalysis.Share.Bar bar)
        {
            const double SmallValue = 1e-10;
   
            double pv, nv, zv;
            
            if (_firstBar)
            {
                pv = nv = SmallValue;
                zv = bar.Volume;
            }
            else
            {
                if (bar.ClosePrice > _prevClosePrice)
                {
                    pv = bar.Volume;
                    nv = zv = 0.0;
                }
                else if (bar.ClosePrice < _prevClosePrice)
                {
                    nv = bar.Volume;
                    pv = zv = 0.0;
                }
                else
                {
                    zv = bar.Volume;
                    pv = nv = 0.0;
                }
            }

            double msPv = _msPv.Update(pv) + SmallValue;
            double msNv = _msNv.Update(nv) + SmallValue;
            double msZv = _msZv.Update(zv) + SmallValue;

            double result = (msPv + msZv / 2.0) / (msNv + msZv / 2.0) * 100.0;

            // update status
            _prevClosePrice = bar.ClosePrice;
            _firstBar = false;

            // return result;
            return result;
        }
    }
}
