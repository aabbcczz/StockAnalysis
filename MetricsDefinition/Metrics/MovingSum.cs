namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("MS")]
    public sealed class MovingSum : SingleOutputRawInputSerialMetric
    {
        private double _sum;

        public MovingSum(int windowSize)
            : base(windowSize)
        {
        }

        public override void Update(double dataPoint)
        {
            _sum += dataPoint;

            if (Data.Length >= WindowSize)
            {
                _sum -= Data[0];
            }

            Data.Add(dataPoint);

            SetValue(_sum);
        }

    }
}
