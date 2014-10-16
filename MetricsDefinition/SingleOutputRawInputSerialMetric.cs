namespace MetricsDefinition
{
    public abstract class SingleOutputRawInputSerialMetric : RawInputSerialMetric
    {
        public SingleOutputRawInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double Update(double dataPoint);
    }
}
