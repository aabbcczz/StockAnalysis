namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("HI_PERIOD")]
    public sealed class HighestPeriod : SingleOutputRawInputSerialMetric
    {
        public HighestPeriod(int windowSize)
            : base(windowSize)
        {
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);

            int period = 1;
            for (int i = -1; i > -Data.Length; --i)
            {
                if (Data[i] > dataPoint)
                {
                    break;
                }

                ++period;
            }

            SetValue(period);
        }
    }
}
