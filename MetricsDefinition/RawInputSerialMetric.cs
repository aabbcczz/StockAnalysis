namespace MetricsDefinition
{
    public abstract class RawInputSerialMetric : SerialMetric
    {
        private readonly CirculatedArray<double> _data;

        internal CirculatedArray<double> Data
        {
            get { return _data; }
        }

        protected RawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            if (windowSize > 0)
            {
                _data = new CirculatedArray<double>(windowSize);
            }
        }
    }
}
