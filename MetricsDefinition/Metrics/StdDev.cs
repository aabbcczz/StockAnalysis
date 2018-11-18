using System;

namespace StockAnalysis.MetricsDefinition.Metrics
{
    [Metric("SD")]
    public sealed class StdDev : SingleOutputRawInputSerialMetric
    {
        public StdDev(int windowSize)
            : base(windowSize)
        {
        }

        public override void Update(double dataPoint)
        {
            Data.Add(dataPoint);

            Values[0] = CalculateStdDev();
        }

        private double CalculateStdDev()
        {
            if (Data.Length <= 1)
            {
                return 0.0;
            }

            var sum = 0.0;
            for (var i = 0; i < Data.Length; ++i)
            {
                sum += Data[i];
            }

            var average = sum / Data.Length;
            var sumOfSquares = 0.0;

            for (var i = 0; i < Data.Length; ++i)
            {
                var data = Data[i];
                sumOfSquares += (data - average) * (data - average);
            }

            return Math.Sqrt(sumOfSquares / (Data.Length - 1));
        }
    }
}
