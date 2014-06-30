using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    [Metric("PSY")]
    public sealed class PsychologicalLine : Metric
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

        public override double[][] Calculate(double[][] input)
        {
 	        if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            double[] allData = input[0];
            double[] up = new double[allData.Length];

            up[0] = 0.0;
            for (int i = 1; i < up.Length; ++i)
            {
                if (allData[i] > allData[i - 1])
                {
                    up[i] = 100.0;
                }
            }

            return new MovingAverage(_lookback).Calculate(new double[1][] { up });
        }
    }
}
