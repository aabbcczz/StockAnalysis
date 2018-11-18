namespace StockAnalysis.MetricsDefinition
{
    using StockAnalysis.Common.Data;

    public abstract class BarInputSerialMetric : SerialMetric
    {
        private readonly CirculatedArray<Bar> _data;

        public virtual CirculatedArray<Bar> Data
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
