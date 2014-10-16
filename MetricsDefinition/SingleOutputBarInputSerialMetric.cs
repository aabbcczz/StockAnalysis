using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class SingleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        public SingleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double Update(Bar bar);
    }
}
