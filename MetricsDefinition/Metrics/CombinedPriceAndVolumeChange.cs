namespace StockAnalysis.MetricsDefinition.Metrics
{
    using Common.Data;

    [Metric("CPVC")]
    public sealed class CombinedPriceAndVolumeChange : SingleOutputBarInputSerialMetric
    {
        private VolumeChange _vc;
        private RateOfChange _roc;

        private static double[] _rocNormalizationTable = new double[]
        {
            -1000.0, 
            0.0, 1.0, 2.0, 3.0, 4.0, 
            4.5, 5.5, 6.0, 6.5, 
            6.8, 
            7.0, 7.2, 7.4, 7.6, 7.8, 8.0, 8.2, 8.4, 8.6, 8.8, 9.0,
            9.2, 9.4, 9.6, 9.8, 9.9,
            1000.0
        };

        public CombinedPriceAndVolumeChange(int volumeLookbackWindow)
            : base(0)
        {
            _vc = new VolumeChange(volumeLookbackWindow);
            _roc = new RateOfChange(1);
        }

        public override void Update(Bar bar)
        {
            _vc.Update(bar);
            _roc.Update(bar.ClosePrice);

            //int index = Array.BinarySearch<double>(_rocNormalizationTable, _roc.Value);
            //if (index < 0)
            //{
            //    index = ~index;
            //    --index;
            //}

            //var combinedValue = index * 10000.0 + _vc.Value;

            var combinedValue = _roc.Value * _vc.Value;
            SetValue(combinedValue);
        }
    }
}
