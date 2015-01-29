using System;

namespace MetricsDefinition.Metrics
{
    [Metric("VE")]
    public sealed class VolatileEnergy : SingleOutputRawInputSerialMetric
    {
        private MovingAverage _ma;

        public VolatileEnergy(int windowSize)
            : base(windowSize)
        {
            if (windowSize <= 1)
            {
                throw new ArgumentException("window size must be greater than 1");
            }

            _ma = new MovingAverage(windowSize);
        }

        public override void Update(double dataPoint)
        {
            _ma.Update(dataPoint);

            Data.Add(dataPoint);

            if (Data.Length <= 2)
            {
                SetValue(0.0);
            }
            else
            {
                int frequency = 1;

                double[] changes = new double[WindowSize];
                int changesIndex = 0;

                double previousData = Data[0];
                double anchor = previousData;
                bool up = Data[1] >= Data[0];

                for (int i = 1; i <= Data.Length; ++i)
                {
                    // add a virtual end point to make the program clear
                    double currentData = i != Data.Length
                        ? Data[i]
                        : (up ? previousData - 1.0 : previousData + 1.0)
                        ;

                    if ((up && currentData < previousData) 
                        || (!up && currentData >= previousData))
                    {
                        double range = previousData - anchor;
                        if (Math.Abs(range) > 1e-6)
                        {
                            changes[changesIndex++] = previousData - anchor;
                        }

                        anchor = previousData;
                        up = !up;

                        ++frequency;
                    }

                    previousData = currentData;
                }

                // calculate std dev for changes
                var sum = 0.0;
                for (var i = 0; i < changesIndex; ++i)
                {
                    sum += changes[i];
                }

                var average = sum / changesIndex;
                var sumOfSquares = 0.0;

                for (var i = 0; i < changesIndex; ++i)
                {
                    var data = changes[i];
                    sumOfSquares += (data - average) * (data - average);
                }

                var stddev = Math.Sqrt(sumOfSquares / (changesIndex - 1));
                
                SetValue(stddev * frequency / _ma.Value);
            }
        }
     }
}
