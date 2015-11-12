namespace MetricsDefinition
{
    public abstract class SingleOutputRawInputSerialMetric : RawInputSerialMetric
    {
        protected SingleOutputRawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            Values = new double[1];
        }

        public abstract void Update(double dataPoint);
    }
}
