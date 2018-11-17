using System;
using StockAnalysis.Common.Data;

namespace MetricsDefinition.Metrics
{
    [Metric("DMI", "PDI,NDI,ADX,ADXR")]
    public sealed class DirectionMovementIndex : MultipleOutputBarInputSerialMetric
    {
        private Bar _prevBar;
        private bool _firstBar = true;

        private readonly MovingSum _msPdm;
        private readonly MovingSum _msNdm;
        private readonly MovingSum _msTr;
        private readonly MovingAverage _maDx;
        private readonly CirculatedArray<double> _adx;

        public DirectionMovementIndex(int windowSize)
            : base (1)
        {
            _msPdm = new MovingSum(windowSize);
            _msNdm = new MovingSum(windowSize);
            _msTr = new MovingSum(windowSize);
            _maDx = new MovingAverage(windowSize);
            _adx = new CirculatedArray<double>(windowSize);

            Values = new double[4];
        }

        public override void Update(Bar bar)
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
            double tr;

            if (_firstBar)
            {
                tr = bar.HighestPrice - bar.LowestPrice;
            }
            else
            {
                tr = Math.Max(Math.Abs(bar.HighestPrice - bar.LowestPrice),
                        Math.Max(Math.Abs(bar.HighestPrice - _prevBar.ClosePrice), Math.Abs(bar.LowestPrice - _prevBar.ClosePrice)));
            }

            // calculate +DIM and -DIM
            _msPdm.Update(pdm);
            _msNdm.Update(ndm);
            _msTr.Update(tr);
            var mspdm = _msPdm.Value;
            var msndm = _msNdm.Value;
            var mstr = _msTr.Value;

            var pdim = mspdm * 100.0 / mstr;
            var ndim = msndm * 100.0 / mstr;

            // calculate DX and ADX
            var dx = Math.Abs((pdim + ndim)) < 1e-6 ? 0.0 : Math.Abs(pdim - ndim) / (pdim + ndim);
            dx *= 100.0;

            _maDx.Update(dx);
            var adx = _maDx.Value;

            // calculate ADXR
            _adx.Add(adx);
            var adxr = _adx.Length < 2 ? _adx[0] : (_adx[-1] + _adx[0]) / 2.0;

            // update internal status
            _prevBar = bar;
            _firstBar = false;

            // return result
            SetValue(pdim, ndim, adx, adxr);
        }
    }
}
