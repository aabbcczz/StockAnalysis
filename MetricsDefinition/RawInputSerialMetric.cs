namespace MetricsDefinition
{
    public abstract class RawInputSerialMetric : SerialMetric
    {
        private readonly CirculatedArray<double> _data;

        internal CirculatedArray<double> Data
        {
            get { return _data; }
        }

        public RawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            _data = new CirculatedArray<double>(windowSize);
        }
    }
}
