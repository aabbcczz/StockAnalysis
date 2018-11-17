using StockAnalysis.Common.Data;

namespace MetricsDefinition
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
