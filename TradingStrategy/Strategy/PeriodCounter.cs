using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.TradingStrategy;

namespace StockAnalysis.TradingStrategy.Strategy
{
    internal sealed class PeriodCounter<T>
    {
        private readonly Dictionary<int, int> _periods = new Dictionary<int, int>();
        private readonly Dictionary<int, T> _objects = new Dictionary<int, T>();

        public void InitializeOrUpdate(ITradingObject tradingObject, int initialPeriod, T obj = default(T))
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            int index = tradingObject.Index;
            if (!_periods.ContainsKey(index))
            {
                _periods.Add(index, initialPeriod);
                _objects.Add(index, obj);
            }
            else
            {
                _periods[index] += 1;
            }
        }

        public void Remove(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            _periods.Remove(tradingObject.Index);
            _objects.Remove(tradingObject.Index);
        }

        public bool Exists(ITradingObject tradingObject)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            return _periods.ContainsKey(tradingObject.Index);
        }

        /// <summary>
        /// Get period of specific trading object
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <returns>period. if trading object does not exists, return -1</returns>
        public int GetPeriod(ITradingObject tradingObject)
        {
            T obj;
            return GetPeriod(tradingObject, out obj);
        }

        /// <summary>
        /// Get period of specific trading object
        /// </summary>
        /// <param name="tradingObject">trading object</param>
        /// <param name="obj">object associated with trading object</param>
        /// <returns>period. if trading object does not exists, return -1</returns>
        public int GetPeriod(ITradingObject tradingObject, out T obj)
        {
            if (tradingObject == null)
            {
                throw new ArgumentNullException("tradingObject");
            }

            int index = tradingObject.Index;
            if (_periods.ContainsKey(index))
            {
                obj = _objects[index];
                return _periods[index];
            }
            else
            {
                obj = default(T);
                return -1;
            }
        }
    }
}
