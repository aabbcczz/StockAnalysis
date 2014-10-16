using StockAnalysis.Share;

namespace MetricsDefinition
{
    public abstract class SingleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        protected SingleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract double Update(Bar bar);
    }
}
