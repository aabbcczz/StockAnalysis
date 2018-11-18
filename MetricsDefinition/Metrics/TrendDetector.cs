namespace StockAnalysis.MetricsDefinition.Metrics
{
    using System;
    using System.Linq;

    [Metric("TD")]
    public sealed class TrendDetector : SingleOutputRawInputSerialMetric
    {
        public TrendDetector(int windowSize)
            : base(windowSize)
        {
            if (windowSize < 3)
            {
                throw new ArgumentOutOfRangeException("window size must not be smaller than 3");
            }
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);

            double trendIndicator = 0.0;

            if (Data.Length >= WindowSize)
            {
                var firstData = Data[0];
                var lastData = Data[-1];

                if (firstData < lastData)
                {
                    // maybe up trend
                    if (firstData == Data.InternalData.Min() && lastData == Data.InternalData.Max())
                    {
                        trendIndicator = 1.0;
                    }
                }
                else if (firstData > lastData)
                {
                    // maybe down trend
                    if (lastData == Data.InternalData.Min() && firstData == Data.InternalData.Max())
                    {
                        trendIndicator = -1.0;
                    }
                }
            }

            SetValue(trendIndicator);
        }
    }
}
