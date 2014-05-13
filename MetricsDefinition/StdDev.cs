using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public class StdDev : SequentialValueMetric<double, double>
    {
        private int _days;

        public StdDev(int days)
            : base (days)
        {
            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException("days must be greater than zero");
            }

            _days = days;
        }

        public override IEnumerable<double> Calculate(IEnumerable<double> input)
        {
            if (input == null || input.Count() == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input.ToArray();

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _days - 1)
                {
                    yield return double.NaN;
                }
                else
                {
                    double sum = 0.0;

                    for (int j = i - _days + 1; j <= i; ++j)
                    {
                        sum += allData[j];
                    }

                    double average = sum / _days;
                    double sumOfSquares = 0;

                    for (int j = i - _days + 1; j <= i; ++j)
                    {
                        sumOfSquares += (allData[j] - average) * (allData[j] - average);
                    }

                    yield return Math.Sqrt(sumOfSquares / _days);
                }
            }
        }
    }
}
