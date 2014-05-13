using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public class MovingAverage : SequentialValueMetric<double, double>
    {
        private int _days;

        public MovingAverage(int days)
        {
            if (days <= 0)
            {
                throw new ArgumentOutOfRangeException("days");
            }

            _days = days;
        }

        public override IEnumerable<double> Calculate(IEnumerable<double> input)
        {
            double sum = 0.0;

            double[] allData = input.ToArray();

            for (int i = 0; i < allData.Length; ++i)
            {
                if (i < _days - 1)
                {
                    yield return double.NaN;
                }
                else if (i == _days - 1)
                {
                    sum = input.Take(_days).Sum();
                    yield return sum / _days;
                }
                else
                {
                    sum = sum - allData[i - _days] + allData[i];
                    yield return sum / _days;
                }
            }
        }
    }
}
