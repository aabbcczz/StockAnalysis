using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("PSY")]
    class PsychologicalLine : IMetric
    {
        private int _lookback;
        
        public PsychologicalLine(int lookback)
        {
            // lookback 0 means infinity lookback
            if (lookback <= 0)
            {
                throw new ArgumentException("lookback must be greater than 0");
            }

            _lookback = lookback;
        }

        public double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input[0];
            int[] up = new int[allData.Length];

            up[0] = 0;
            for (int i = 1; i < up.Length; ++i)
            {
                if (allData[i] > allData[i - 1])
                {
                    up[i] = 1;
                }
            }

            double sum = 0.0;

            double[] result = new double[up.Length];

            for (int i = 0; i < up.Length; ++i)
            {
                if (i < _lookback)
                {
                    sum += up[i];
                    result[i] = sum * 100.0 / (i + 1);
                }
                else
                {
                    int j = i - _lookback + 1;

                    sum += up[i] - up[j];
                    result[i] = sum * 100.0 / _lookback;
                }
            }

            return new double[1][] { result };
        }
    }
}
