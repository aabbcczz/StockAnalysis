using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class MultipleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        public MultipleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double[] Update(Bar bar);
    }
}
