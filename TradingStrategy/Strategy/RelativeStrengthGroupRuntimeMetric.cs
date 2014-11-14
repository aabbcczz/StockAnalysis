using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy.Strategy
{
    public class RelativeStrengthGroupRuntimeMetric : IGroupRuntimeMetric
    {
        private readonly ITradingObject[] _tradingObjects;
        private readonly double[] _values = new double[1];
        private readonly int[] _orderOfTradingObjectMetricValues;
        private readonly string[] _dependedMetrics;
        private readonly double[] _rocValues;
        private readonly int[] _rocOrders;

        public double[] GroupMetricValues
        {
            get { return _values; }
        }

        public int[] OrderOfTradingObjectMetricValues
        {
            get { return _orderOfTradingObjectMetricValues; }
        }

        public IEnumerable<string> DependedMetrics
        {
            get { return _dependedMetrics; }
        }

        public IEnumerable<ITradingObject> GroupedTradingObjects
        {
            get { return _tradingObjects; }
        }

        public void Update(IRuntimeMetric[][] metrics)
        {
            if (metrics == null)
            {
                throw new ArgumentNullException();
            }

            // set faked group metric value.
            _values[0] = 0.0;

            // order the trading object's metric value
            IRuntimeMetric[] rocMetrics = metrics[0];
            for (int i = 0; i < _rocValues.Length; ++i)
            {
                _rocOrders[i] = i;
                _rocValues[i] = rocMetrics[i] == null ? double.MinValue : rocMetrics[i].Values[0];
            }

            // sort roc values in ascending order
            Array.Sort(_rocValues, _rocOrders);

            int count = _tradingObjects.Length;
            for (int i = 0; i <_rocOrders.Length; ++i)
            {
                // we need to set order according to the roc value in descending vlaue,
                // so we need to use (count - 1 - i) as true order here.
                _orderOfTradingObjectMetricValues[_rocOrders[i]] = count - 1 - i;
            }
        }

        public RelativeStrengthGroupRuntimeMetric(IEnumerable<ITradingObject> tradingObjects, int rocWindowSize)
        {
            _tradingObjects = tradingObjects.ToArray();
            _orderOfTradingObjectMetricValues = new int[_tradingObjects.Length];
            _rocValues = new double[_tradingObjects.Length];
            _rocOrders = new int[_tradingObjects.Length];

            _dependedMetrics = new string[]
            {
                string.Format("ROC[{0}]", rocWindowSize)
            };
        }
    }
}
