using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    [Metric("SD")]
    public sealed class StdDev : SingleOutputRawInputSerialMetric
    {
        public StdDev(int windowSize)
            : base(windowSize)
        {
        }

        public override double Update(double dataPoint)
        {
            Data.Add(dataPoint);

            return CalculateStdDev();
        }

        private double CalculateStdDev()
        {
            if (Data.Length <= 1)
            {
                return 0.0;
            }

            double sum = 0.0;
            for (int i = 0; i < Data.Length; ++i)
            {
                sum += Data[i];
            }

            double average = sum / Data.Length;
            double sumOfSquares = 0.0;

            for (int i = 0; i < Data.Length; ++i)
            {
                double data = Data[i];
                sumOfSquares += (data - average) * (data - average);
            }

            return Math.Sqrt(sumOfSquares / (Data.Length - 1));
        }
    }
}
