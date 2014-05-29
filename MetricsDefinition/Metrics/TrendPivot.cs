using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MetricsDefinition
{
    [Metric("TP")]
    public sealed class TrendPivot : Metric
    {
        private double _trendTolerance;

        public TrendPivot(double trendTolerance)
        {
            if (trendTolerance < 0.0 || _trendTolerance > 1.0)
            {
                throw new ArgumentOutOfRangeException("trendTolerance must be in [0.0, 1.0]");
            }
        }

        public override double[][] Calculate(double[][] input)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException("input");
            }

            // find trends point
            FindTrendPivots(input[0]);


            double[] result = new double[1];

            return new double[1][] { result };
        }

        private double[] FindTrendPivots(double[] data)
        {
            if (data == null || data.Length < 2)
            {
                throw new ArgumentOutOfRangeException("data");
            }

            double[] pivots = null;

            // first step is to find out the raw trends
            List<KeyValuePair<int, double>> rawTrendPivots = new List<KeyValuePair<int,double>>(data.Length);

            int startIndex = 0;
            while (startIndex < data.Length)
            {
                rawTrendPivots.Add(new KeyValuePair<int, double>(startIndex, data[startIndex]));

                if (startIndex == data.Length - 1)
                {
                    break;
                }

                startIndex = FindNextPivot(data, startIndex);
            }

            System.Diagnostics.Debug.Assert(rawTrendPivots.Count >= 2);

            // handle the first straight trend if any
            if (rawTrendPivots[1].Value == rawTrendPivots[0].Value)
            {
                if (rawTrendPivots.Count == 2)
                {
                    // all data are the same
                    pivots = new double[data.Length];
                    pivots[0] = data[0];
                    pivots[data.Length - 1] = data[data.Length - 1];

                    return pivots;
                }
                else
                {
                    // merge the first trend with the second trend
                    rawTrendPivots.RemoveAt(1);
                }
            }

            System.Diagnostics.Debug.Assert(rawTrendPivots.Count >= 2);

            // compress pivots based on trend tolerance value
            int startPivotIndex = 0;
            List<KeyValuePair<int, double>> trendPivots = new List<KeyValuePair<int, double>>(rawTrendPivots.Count);

            while (startPivotIndex < rawTrendPivots.Count)
            {
                trendPivots.Add(rawTrendPivots[startPivotIndex]);

                if (startPivotIndex == rawTrendPivots.Count - 1)
                {
                    // last one pivot, nothing to do more.
                    break;
                }


            }

            pivots = new double[data.Length];

            pivots[0] = data[0];

            int pivotIndex = 0;

            do
            {
                pivotIndex = FindNextPivot(data, pivotIndex);

                pivots[pivotIndex] = data[pivotIndex];

            } while (pivotIndex < data.Length - 1);

            return pivots;
        }

        int FindNextPivot(double[] data, int startIndex)
        {
            if (data == null || data.Length <= startIndex)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (startIndex == data.Length - 1)
            {
                return -1;
            }

            Func<double, double, bool>[] predictes = new Func<double, double, bool>[3]
            {
                (d1, d2) => { return d1 > d2; },
                (d1, d2) => { return d1 < d2; },
                (d1, d2) => { return d1 != d2; }
            };

            Func<double, double, bool> activePredicate = null;

            if (data[startIndex] < data[startIndex + 1]) // upward trend
            {
                activePredicate = predictes[0];
            }
            else if (data[startIndex] > data[startIndex + 1]) // downward trend
            {
                activePredicate = predictes[1];
            }
            else // straight trend at beginning
            {
                activePredicate = predictes[2];
            }

            int nextIndex = -1;

            for (int i = startIndex; i < data.Length - 1; ++i)
            {
                if (activePredicate(data[i], data[i + 1]))
                {
                    nextIndex = i;
                    break;
                }
            }

            if (nextIndex < 0)
            {
                nextIndex = data.Length - 1;
            }

            return nextIndex;
        }

        int FindNextPivot(IList<KeyValuePair<int, double>> pivots, int startIndex)
        {
            if (pivots == null || pivots.Count <= startIndex)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (startIndex == pivots.Count - 1)
            {
                return -1;
            }

            if (startIndex == pivots.Count - 2)
            {
                return pivots.Count - 1;
            }

            return -1;
        }
    }
}
