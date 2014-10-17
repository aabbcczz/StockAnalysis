namespace MetricsDefinition
{
    public abstract class MultipleOutputRawInputSerialMetric : RawInputSerialMetric
    {
        protected MultipleOutputRawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double[] Update(double dataPoint);
    }
}
