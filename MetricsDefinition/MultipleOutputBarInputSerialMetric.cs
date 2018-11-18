using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition
{
    public abstract class MultipleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        protected MultipleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
        }

        public abstract void Update(Bar bar);
    }
}
