using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using StockAnalysis.Share;

namespace MetricsDefinition
{
    [Metric("DMI", "PDI,NDI,ADX,ADXR")]
    public sealed class DirectionMovementIndex : MultipleOutputBarInputSerialMetric
    {
        private Bar _prevBar;
        private bool _firstBar = true;

        private MovingSum _msPdm;
        private MovingSum _msNdm;
        private MovingSum _msTr;
        private MovingAverage _maDx;
        private CirculatedArray<double> _adx;

        public DirectionMovementIndex(int windowSize)
            : base (1)
        {
            _msPdm = new MovingSum(windowSize);
            _msNdm = new MovingSum(windowSize);
            _msTr = new MovingSum(windowSize);
            _maDx = new MovingAverage(windowSize);
            _adx = new CirculatedArray<double>(windowSize);
        }

        public override double[] Update(StockAnalysis.Share.Bar bar)
        {
            // calculate +DM and -DM
            double pdm, ndm;

            if (_firstBar)
            {
                pdm = 0.0;
                ndm = 0.0;
            }
            else
            {
                pdm = Math.Max(0.0, bar.HighestPrice - _prevBar.HighestPrice);
                ndm = Math.Max(0.0, _prevBar.LowestPrice - bar.LowestPrice);

                if (pdm > ndm)
                {
                    ndm = 0.0;
                }
                else if (pdm < ndm)
                {
                    pdm = 0.0;
                }
                else
                {
                    pdm = ndm = 0.0;
                }
            }

            // Calculate +DI and -DI
            double tr, pdi, ndi;

            if (_firstBar)
            {
                tr = bar.HighestPrice - bar.LowestPrice;
            }
            else
            {
                tr = Math.Max(Math.Abs(bar.HighestPrice - bar.LowestPrice),
                        Math.Max(Math.Abs(bar.HighestPrice - _prevBar.ClosePrice), Math.Abs(bar.LowestPrice - _prevBar.ClosePrice)));
            }

            pdi = pdm * 100.0 / tr;
            ndi = ndm * 100.0 / tr;

            // calculate +DIM and -DIM
            double mspdm = _msPdm.Update(pdm);
            double msndm = _msNdm.Update(ndm);
            double mstr = _msTr.Update(tr);

            double pdim = mspdm * 100.0 / mstr;
            double ndim = msndm * 100.0 / mstr;

            // calculate DX and ADX
            double dx = (pdim + ndim) == 0.0 ? 0.0 : Math.Abs(pdim - ndim) / (pdim + ndim);
            dx *= 100.0;

            double adx = _maDx.Update(dx);

            // calculate ADXR
            _adx.Add(adx);
            double adxr = _adx.Length < 2 ? _adx[0] : (_adx[-1] + _adx[0]) / 2.0;

            // update internal status
            _prevBar = bar;
            _firstBar = false;

            // return result
            return new double[4] { pdim, ndim, adx, adxr };
        }
    }
}
