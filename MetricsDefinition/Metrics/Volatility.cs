using System;

namespace MetricsDefinition.Metrics
{
    [Metric("VOLA")]
    public sealed class Volatility : SingleOutputRawInputSerialMetric
    {
        public Volatility(int windowSize)
            : base(windowSize)
        {
            if (windowSize <= 2)
            {
                throw new ArgumentException("windowSize must be greater than 2");
            }
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);

            if (Data.Length < WindowSize)
            {
                SetValue(0.0);
            }
            else
            {
                double v0 = Data[0];
                double vN = Data[-1];

                double slope = (vN - v0) / v0 / (Data.Length - 1);
                double variance = 0.0;

                for (int i = 1; i < Data.Length - 1; ++i)
                {
                    // normalize data point
                    double v = Data[i] / v0;
                    double diff = v - slope * i - 1.0;

                    variance += diff * diff;
                }

                double volatility = Math.Sqrt(variance / (Data.Length - 2));

                SetValue(volatility);
            }
        }
     }
}
