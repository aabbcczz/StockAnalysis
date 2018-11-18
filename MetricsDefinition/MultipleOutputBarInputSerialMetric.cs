namespace StockAnalysis.MetricsDefinition
{
    using Common.Data;

    public abstract class MultipleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        protected MultipleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract void Update(Bar bar);
    }
}
