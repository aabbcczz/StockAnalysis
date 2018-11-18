namespace StockAnalysis.TradingStrategy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class MetricGroupSorter
    {
        public sealed class DefaultAscendingOrderComparer : IComparer<double[]>
        {
            private readonly int _valueIndex;

            public DefaultAscendingOrderComparer(int valueIndex = 0)
            {
                _valueIndex = valueIndex;
            }

            public int Compare(double[] x, double[] y)
            {
                return x[_valueIndex].CompareTo(y[_valueIndex]);
            }
        }

        public sealed class DefaultDescendingOrderComparer : IComparer<double[]>
        {
            private readonly int _valueIndex;

            public DefaultDescendingOrderComparer(int valueIndex = 0)
            {
                _valueIndex = valueIndex;
            }

            public int Compare(double[] x, double[] y)
            {
                return y[_valueIndex].CompareTo(x[_valueIndex]);
            }
        }

        private readonly ITradingObject[] _tradingObjects;
        private readonly double[][] _values;
        private readonly int[] _finalOrders;
        private readonly int[] _facilityOrders;
        private double[] _defaultMetricValues = null;
        private readonly Dictionary<int, int> _tradingObjectIndexToLocalIndexMap;

        public IEnumerable<ITradingObject> TradingObjects
        {
            get { return _tradingObjects; }
        }

        public int Count
        {
            get { return _tradingObjects.Length; }
        }

        public int[] LatestOrders
        {
            get { return _finalOrders; }
        }

        public int this[ITradingObject tradingObject]
        {
            get { return _finalOrders[_tradingObjectIndexToLocalIndexMap[tradingObject.Index]]; }
        }

        public MetricGroupSorter(IEnumerable<ITradingObject> tradingObjects)
        {
            if (tradingObjects == null)
            {
                throw new ArgumentNullException();
            }

            _tradingObjects = tradingObjects.ToArray();
            _finalOrders = new int[_tradingObjects.Length];
            _facilityOrders = new int[_tradingObjects.Length];
            _values = new double[_tradingObjects.Length][];

            _tradingObjectIndexToLocalIndexMap = Enumerable
                .Range(0, _tradingObjects.Length)
                .ToDictionary(i => _tradingObjects[i].Index);
        }

        public void OrderByAscending(IRuntimeMetric[] metrics)
        {
            OrderBy(metrics, new DefaultAscendingOrderComparer());
        }

        public void OrderByAscending(IRuntimeMetric[] metrics, int valueIndex)
        {
            OrderBy(metrics, new DefaultAscendingOrderComparer(valueIndex));
        }

        public void OrderByDescending(IRuntimeMetric[] metrics)
        {
            OrderBy(metrics, new DefaultDescendingOrderComparer());
        }

        public void OrderByDescending(IRuntimeMetric[] metrics, int valueIndex)
        {
            OrderBy(metrics, new DefaultDescendingOrderComparer(valueIndex));
        }

        public void OrderBy(IRuntimeMetric[] metrics, IComparer<double[]> comparer)
        {
            if (!PrepareForOrdering(metrics))
            {
                for (int i = 0; i < _finalOrders.Length; ++i)
                {
                    _finalOrders[i] = i;
                }
            }
            else
            {
                Array.Sort(_values, _facilityOrders, comparer);

                for (int i = 0; i < _finalOrders.Length; ++i)
                {
                    _finalOrders[_facilityOrders[i]] = i;
                }
            }
        }

        private bool PrepareForOrdering(IRuntimeMetric[] metrics)
        {
            if (metrics == null)
            {
                throw new ArgumentNullException();
            }

            if (metrics.Length != _tradingObjects.Length)
            {
                throw new ArgumentException("number of metrics does not match number of trading objects");
            }

            if (_defaultMetricValues == null)
            {
                var firstRealMetric = metrics.FirstOrDefault(m => m != null);
                if (firstRealMetric == null)
                {
                    return false;
                }

                _defaultMetricValues = new double[firstRealMetric.Values.Length];
            }

            for (int i = 0; i < _facilityOrders.Length; ++i)
            {
                var metric = metrics[i];

                _values[i] = metric != null ? metrics[i].Values : _defaultMetricValues;
                _facilityOrders[i] = i;
            }

            return true;
        }
    }
}
