namespace StockAnalysis.MetricsDefinition.Metrics
{
    using Common.Data;

    // Volatility Volume Ratio
    [Metric("VR")]
    public sealed class VolumeRatio : SingleOutputBarInputSerialMetric
    {
        private double _prevClosePrice;
        private bool _firstBar = true;

        private readonly MovingSum _msPv;
        private readonly MovingSum _msNv;
        private readonly MovingSum _msZv;

        public VolumeRatio(int windowSize)
            : base(0)
        {
            _msPv = new MovingSum(windowSize);
            _msNv = new MovingSum(windowSize);
            _msZv = new MovingSum(windowSize);
        }

        public override void Update(Bar bar)
        {
            const double smallValue = 1e-10;
   
            double pv, nv, zv;
            
            if (_firstBar)
            {
                pv = nv = smallValue;
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

            _msPv.Update(pv);
            _msNv.Update(nv);
            _msZv.Update(zv);

            var msPv = _msPv.Value + smallValue;
            var msNv = _msNv.Value + smallValue;
            var msZv = _msZv.Value + smallValue;

            var volumeRatio = (msPv + msZv / 2.0) / (msNv + msZv / 2.0) * 100.0;

            // update status
            _prevClosePrice = bar.ClosePrice;
            _firstBar = false;

            // return result;
            SetValue(volumeRatio);
        }
    }
}
