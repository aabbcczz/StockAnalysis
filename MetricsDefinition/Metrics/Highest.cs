namespace MetricsDefinition.Metrics
{
    [Metric("HI", "VALUE,INDEX")]
    public sealed class Highest : MultipleOutputRawInputSerialMetric
    {
        private double _highestPrice = double.MinValue;
        private int _highestPriceIndex = -1;

        public Highest(int windowSize)
            : base(windowSize)
        {
            Values = new double[2];
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);
            --_highestPriceIndex;

            if (dataPoint >= _highestPrice)
            {
                _highestPrice = dataPoint;
                _highestPriceIndex = Data.Length - 1;
            }
            else
            {
                // determine if current highest price is still valid
                if (_highestPriceIndex < 0)
                {
                    _highestPrice = double.MinValue;
                    _highestPriceIndex = -1;
                    for (var i = 0; i < Data.Length; ++i)
                    {
                        var data = Data[i];
                        if (data >= _highestPrice)
                        {
                            _highestPrice = data;
                            _highestPriceIndex = i;
                        }
                    }
                }
            }

            SetValue(_highestPrice, _highestPriceIndex);
        }
     }
}
