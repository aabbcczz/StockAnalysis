namespace StockAnalysis.StockTrading.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Common.Utility;

    sealed class OrderDispatcher
    {
        private readonly int _refreshingIntervalInMillisecond;

        private TradingClient _client = null;

        private object _dispatcherLockObj = new object();
        private object _orderLockObj = new object();

        private Timer _timer = null;
        private bool _isStopped = false;

        private IDictionary<int, DispatchedOrder> _allActiveOrders = new Dictionary<int, DispatchedOrder>();

        public OrderDispatcher(TradingClient client, int refreshingIntervalInMillisecond)
        {
            if (client == null)
            {
                throw new ArgumentException();
            }

            _client = client;
            _refreshingIntervalInMillisecond = refreshingIntervalInMillisecond;

            _timer = new Timer(QueryOrderStatus, null, 0, _refreshingIntervalInMillisecond);
        }

        public void Stop()
        {
            _timer.Dispose();
            _timer = null;

            lock (_dispatcherLockObj)
            {
                _isStopped = true;

                _client = null;
            }
        }

        public DispatchedOrder DispatchOrder(
            OrderRequest request, 
            WaitableConcurrentQueue<OrderStatusChangedMessage> orderStatusChangedMessageReceiver, 
            out string error)
        {
            if (request == null || orderStatusChangedMessageReceiver == null)
            {
                throw new ArgumentNullException();
            }

            var result = _client.SendOrder(request, out error);

            if (result == null)
            {
                return null;
            }

            DispatchedOrder dispatchedOrder 
                = new DispatchedOrder(request, orderStatusChangedMessageReceiver, result.OrderNo);

            lock (_orderLockObj)
            {
                _allActiveOrders.Add(result.OrderNo, dispatchedOrder);
            }

            return dispatchedOrder.Clone();
        }

        public DispatchedOrder[] DispatchOrder(
            OrderRequest[] requests, 
            WaitableConcurrentQueue<OrderStatusChangedMessage> orderStatusChangedMessageReceiver, 
            out string[] errors)
        {
            if (requests == null || orderStatusChangedMessageReceiver == null)
            {
                throw new ArgumentNullException();
            }

            var results = _client.SendOrder(requests, out errors);

            lock (_orderLockObj)
            {
                DispatchedOrder[] orders = new DispatchedOrder[results.Length];

                for (int i = 0; i < results.Length; ++i)
                {
                    if (results[i] != null)
                    {
                        DispatchedOrder dispatchedOrder
                            = new DispatchedOrder(requests[i], orderStatusChangedMessageReceiver, results[i].OrderNo);

                        _allActiveOrders.Add(results[i].OrderNo, dispatchedOrder);

                        orders[i] = dispatchedOrder.Clone();

                    }
                    else
                    {
                        orders[i] = null;

                        AppLogger.Default.ErrorFormat(
                            "Send order failed. Error: {0}. Order request details: {1}", 
                            errors[i],
                            requests[i]);
                    }
                }

                return orders;
            }
        }

        public bool CancelOrder(DispatchedOrder order, out string error)
        {
            error = string.Empty;

            if (TradingHelper.IsFinalStatus(order.LastStatus))
            {
                return true;
            }

            bool cancelSucceeded = _client.CancelOrder(order.Request.SecuritySymbol, order.OrderNo, out error);

            return cancelSucceeded;
        }

        public bool[] CancelOrder(DispatchedOrder[] orders, out string[] errors)
        {
            var symbols = orders.Select(o => o.Request.SecuritySymbol).ToArray();
            var orderNos = orders.Select(o => o.OrderNo).ToArray();

            bool[] succeededFlags = _client.CancelOrder(symbols, orderNos, out errors);

            return succeededFlags;
        }

        public void QueryOrderStatusForcibly()
        {
            // force query order status
            QueryOrderStatus(null);
        }

        private void QueryOrderStatus(object state)
        {
            if (!Monitor.TryEnter(_dispatcherLockObj))
            {
                // ignore this refresh because previous refreshing is still on going.
                return;
            }

            try
            {
                if (_isStopped)
                {
                    return;
                }

                if (_client == null || !_client.IsLoggedOn())
                {
                    return;
                }

               bool hasActiveOrder = false;
                lock (_orderLockObj)
                {
                    hasActiveOrder = _allActiveOrders.Count > 0;
                }

                if (!hasActiveOrder)
                {
                    return;
                }

                string error;
                var submittedOrders = _client.QuerySubmittedOrderToday(out error).ToList();

                if (submittedOrders.Count() == 0)
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        AppLogger.Default.WarnFormat("Failed to query submitted order. error: {0}", error);
                    }
                }
                else
                {
                    foreach (var order in submittedOrders)
                    {
                        DispatchedOrder dispatchedOrder;

                        lock (_orderLockObj)
                        {
                            if (!_allActiveOrders.TryGetValue(order.OrderNo, out dispatchedOrder))
                            {
                                // not submitted by the dispatcher or the order is finished, ignore it.
                                continue;
                            }
                        }

                        // check if order status has been changed and notify client if necessary
                        CheckOrderStatusChangeAndNotify(ref dispatchedOrder, order);

                        // remove order in final status
                        if (TradingHelper.IsFinalStatus(dispatchedOrder.LastStatus))
                        {
                            lock (_orderLockObj)
                            {
                                _allActiveOrders.Remove(dispatchedOrder.OrderNo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Default.ErrorFormat("Exception in querying order status: {0}", ex);
            }
            finally
            {
                Monitor.Exit(_dispatcherLockObj);
            }
        }

        private bool CheckOrderStatusChangeAndNotify(ref DispatchedOrder dispatchedOrder, QueryGeneralOrderResult orderResult)
        {
            if (orderResult.Status == OrderStatus.Unknown)
            {
                // log it for debugging and enrich status string
                AppLogger.Default.ErrorFormat("Find unknown order status: {0}", orderResult.StatusString);
            }

            bool isStatusChanged = false;

            OrderStatusChangedMessage message = null;

            if (orderResult.Status != dispatchedOrder.LastStatus)
            {
                isStatusChanged = true;

                message = new OrderStatusChangedMessage()
                {
                    Order = dispatchedOrder,
                    PreviousStatus = dispatchedOrder.LastStatus,
                    CurrentStatus = orderResult.Status
                };
            }
            
            dispatchedOrder.LastStatus = orderResult.Status;
            dispatchedOrder.LastTotalDealVolume = orderResult.DealVolume;
            dispatchedOrder.LastAverageDealPrice = orderResult.DealPrice;

            if (isStatusChanged)
            {
                NotifyOrderStatusChanged(message);
            }

            return isStatusChanged;
        }

        private void NotifyOrderStatusChanged(OrderStatusChangedMessage message)
        {
            message.Order.OrderStatusChangedMessageReceiver.Add(message);
        }
    }
}
