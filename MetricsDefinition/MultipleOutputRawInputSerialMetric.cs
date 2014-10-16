namespace MetricsDefinition
{
    public abstract class MultipleOutputRawInputSerialMetric : RawInputSerialMetric
    {
        public MultipleOutputRawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double[] Update(double dataPoint);
    }
}
