using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockAnalysis.Share;
using StockTrading.Utility;

namespace StockTradingConsole
{
    sealed class OrderStatusTracker
    {
        private object _syncObj = new object();
        private TradingClient _client = null;
        private Dictionary<int, QueryGeneralOrderResult> _orders = new Dictionary<int, QueryGeneralOrderResult>();

        public OrderStatusTracker(TradingClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            _client = client;
        }

        public bool RegisterOrder(int orderNo)
        {
            lock(_syncObj)
            {
                if (_orders.ContainsKey(orderNo))
                {
                    AppLogger.Default.DebugFormat("OrderNo {0} has been registered", orderNo);
                    return false;
                }

                _orders.Add(orderNo, null);
                return true;
            }
        }

        public bool UnregisterOrder(int orderNo)
        {
            lock (_syncObj)
            {
                if (!_orders.ContainsKey(orderNo))
                {
                    AppLogger.Default.DebugFormat("OrderNo {0} has not been registered", orderNo);
                    return false;
                }

                _orders.Remove(orderNo);
                return true;
            }
        }

        public QueryGeneralOrderResult GetOrderStatus(int orderNo)
        {
            lock (_syncObj)
            {
                if (!_orders.ContainsKey(orderNo))
                {
                    AppLogger.Default.DebugFormat("OrderNo {0} has not been registered", orderNo);
                    return null;
                }

                return _orders[orderNo];
            }
        }

        public void UpdateStatus()
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Trading client is null");
            }

            lock (_syncObj)
            {
                // if there is no active order, we will not send the query to server
                int activeOrderCount = _orders.Count(kvp => kvp.Value == null || !TradingHelper.IsFinalStatus(kvp.Value.Status));

                if (activeOrderCount == 0)
                {
                    return;
                }
            }

            string error;

            var results = _client.QuerySubmittedOrderToday(out error);

            if (results == null || results.Count() == 0)
            {
                if (!string.IsNullOrEmpty(error))
                {
                    AppLogger.Default.ErrorFormat("Error in QuerySubmittedOrderToday: {0}", error);
                }

                return;
            }

            lock (_syncObj)
            {
                foreach (var result in results)
                {
                    if (_orders.ContainsKey(result.OrderNo))
                    {
                        _orders[result.OrderNo] = result;
                    }
                }
            }
        }
    }
}
