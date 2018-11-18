using StockAnalysis.Common.Data;

namespace StockAnalysis.MetricsDefinition
{
    public abstract class SingleOutputBarInputSerialMetric : BarInputSerialMetric
    {
        protected SingleOutputBarInputSerialMetric(int windowSize)
            : base(windowSize)
        {
            Values = new double[1];
        }

        public abstract void Update(Bar bar);
    }
}
