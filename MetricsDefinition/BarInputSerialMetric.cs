using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class BarInputSerialMetric : SerialMetric
    {
        private readonly CirculatedArray<Bar> _data;

        internal CirculatedArray<Bar> Data
        {
            get { return _data; }
        }

        protected BarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            if (windowSize > 0)
            {
                _data = new CirculatedArray<Bar>(windowSize);
            }
        }
    }
}
