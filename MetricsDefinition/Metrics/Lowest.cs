namespace MetricsDefinition.Metrics
{
    [Metric("LO", "VALUE,INDEX")]
    public sealed class Lowest : MultipleOutputRawInputSerialMetric
    {
        private double _lowestPrice = double.MaxValue;
        private int _lowestPriceIndex = -1;

        public Lowest(int windowSize)
            : base(windowSize)
        {
            Values = new double[2];
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);
            --_lowestPriceIndex;

            if (dataPoint <= _lowestPrice)
            {
                _lowestPrice = dataPoint;
                _lowestPriceIndex = Data.Length - 1;
            }
            else
            {
                // determine if current lowest price is still valid
                if (_lowestPriceIndex < 0)
                {
                    _lowestPrice = double.MaxValue;
                    _lowestPriceIndex = -1;
                    for (var i = 0; i < Data.Length; ++i)
                    {
                        var data = Data[i];
                        if (data <= _lowestPrice)
                        {
                            _lowestPrice = data;
                            _lowestPriceIndex = i;
                        }
                    }
                }
            }

            SetValue(_lowestPrice, _lowestPriceIndex);
        }
    }
}
