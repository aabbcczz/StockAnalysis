using StockAnalysis.Share;

namespace MetricsDefinition.Metrics
{
    [Metric("REF")]
    public sealed class Reference : SingleOutputRawInputSerialMetric
    {
        public Reference(int reference)
            : base(reference)
        {
            Values = new double[1];
        }

        public override void Update(double dataPoint)
        {
            if (Data.Length == 0)
            {
                SetValue(0.0);
            }
            else
            {
                SetValue(Data[0]);
            }

            Data.Add(dataPoint);
        }
    }
}
