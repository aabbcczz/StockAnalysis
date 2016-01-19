using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using log4net;

namespace StockTrading.Utility
{
    sealed class OrderDispatcher
    {
        private TradingClient _client = null;

        private object _dispatcherLockObj = new object();
        private object _orderLockObj = new object();

        private bool _isStopped = false;

        private IDictionary<int, DispatchedOrder> _orderNoToRequestIdMap = new Dictionary<int, DispatchedOrder>();

        private CtpSimulator.OnOrderFullySucceededDelegate _onOrderFullySucceededCallback = null;

        public OrderDispatcher(TradingClient client)
        {
            if (client == null)
            {
                throw new ArgumentException();
            }

            _client = client;

            ThreadPool.QueueUserWorkItem(QueryOrderStatus);
        }

        public void Stop()
        {
            lock (_dispatcherLockObj)
            {
                _isStopped = true;

                _client = null;
            }
        }

        public void RegisterOrderFullySucceededCallback(CtpSimulator.OnOrderFullySucceededDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }

            lock (_dispatcherLockObj)
            {
                _onOrderFullySucceededCallback += callback;
            }
        }

        public DispatchedOrder DispatchOrder(OrderRequest request, out string error)
        {
            var result = _client.SendOrder(request, out error);

            if (result == null)
            {
                return null;
            }

            DispatchedOrder dispatchedOrder = new DispatchedOrder()
            {
                OrderNo = result.OrderNo,
                Request = request
            };

            lock (_orderLockObj)
            {
                _orderNoToRequestIdMap.Add(result.OrderNo, dispatchedOrder);
            }

            return dispatchedOrder;
        }

        public bool CancelOrder(DispatchedOrder result, out string error)
        {
            bool isCancelled = _client.CancelOrder(result.Request.SecurityCode, result.OrderNo, out error);

            if (isCancelled)
            {
                lock (_orderLockObj)
                {
                    _orderNoToRequestIdMap.Remove(result.OrderNo);
                }
            }

            return isCancelled;
        }

        private void QueryOrderStatus(object state)
        {
            while (!_isStopped)
            {
                Thread.Sleep(1000);

                if (_client == null || _client.IsLoggedOn())
                {
                    continue;
                }

                try
                {
                    string error;
                    var succeededOrders = _client.QuerySucceededOrderToday(out error).ToList();

                    if (succeededOrders.Count() == 0)
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                            if (logger != null)
                            {
                                logger.WarnFormat("Failed to query succeeded order. error: {0}", error);
                            }
                        }
                    }
                    else
                    {
                        foreach (var order in succeededOrders)
                        {
                            DispatchedOrder dispatchedOrder;

                            lock (_orderLockObj)
                            {
                                if (!_orderNoToRequestIdMap.TryGetValue(order.OrderNo, out dispatchedOrder))
                                {
                                    // not submitted by the dispatcher, ignore it.
                                    continue;
                                }
                                else
                                {
                                    _orderNoToRequestIdMap.Remove(order.OrderNo);
                                }
                            }

                            // now we got the request id, prepare to notify client
                            NotifyOrderFullySucceeded(dispatchedOrder);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                    if (logger != null)
                    {
                        logger.ErrorFormat("Exception in querying order status: {0}", ex);
                    }
                }
            }
        }

        private void NotifyOrderFullySucceeded(DispatchedOrder dispatchedOrder)
        {
            if (_onOrderFullySucceededCallback != null)
            {
                _onOrderFullySucceededCallback(dispatchedOrder);
            }
        }
    }
}
