using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using log4net;
using StockAnalysis.Share;

namespace StockTrading.Utility
{
    sealed class OrderDispatcher
    {
        private readonly int _refreshingIntervalInMillisecond;

        private TradingClient _client = null;

        private object _dispatcherLockObj = new object();
        private object _orderLockObj = new object();

        private Timer _timer = null;
        private bool _isStopped = false;

        private IDictionary<int, DispatchedOrder> _allActiveOrders = new Dictionary<int, DispatchedOrder>();

        private CtpSimulator.OnOrderStatusChangedDelegate _onOrderStatusChangedCallback = null;

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

        public void RegisterOrderStatusChangedCallback(CtpSimulator.OnOrderStatusChangedDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }

            lock (_dispatcherLockObj)
            {
                _onOrderStatusChangedCallback += callback;
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
                DispatchTime = DateTime.Now,
                OrderNo = result.OrderNo,
                Request = request,
                LastDealPrice = 0.0f,
                LastDealVolume = 0,
                LastStatus = OrderStatus.NotSubmitted,
            };

            lock (_orderLockObj)
            {
                _allActiveOrders.Add(result.OrderNo, dispatchedOrder);
            }

            return dispatchedOrder;
        }

        public DispatchedOrder[] DispatchOrder(OrderRequest[] requests, out string[] errors)
        {
            var results = _client.SendOrder(requests, out errors);

            lock (_orderLockObj)
            {
                DispatchedOrder[] orders = new DispatchedOrder[results.Length];

                for (int i = 0; i < results.Length; ++i)
                {
                    if (results[i] != null)
                    {
                        DispatchedOrder dispatchedOrder = new DispatchedOrder()
                        {
                            DispatchTime = DateTime.Now,
                            OrderNo = results[i].OrderNo,
                            Request = requests[i],
                            LastDealPrice = 0.0f,
                            LastDealVolume = 0,
                            LastStatus = OrderStatus.NotSubmitted,
                        };

                        _allActiveOrders.Add(results[i].OrderNo, dispatchedOrder);

                        orders[i] = dispatchedOrder;

                    }
                    else
                    {
                        orders[i] = null;
                    }
                }

                return orders.ToArray();
            }
        }

        public bool CancelOrder(DispatchedOrder order, out string error, bool waitForResult)
        {
            bool cancelSucceeded = _client.CancelOrder(order.Request.SecurityCode, order.OrderNo, out error);

            if (!waitForResult)
            {
                return cancelSucceeded;
            }

            if (cancelSucceeded)
            {
                do
                {
                    var submittedOrders = _client.QuerySubmittedOrderToday(out error).ToList();

                    if (submittedOrders.Count() == 0)
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            AppLogger.Default.WarnFormat("Failed to query submitted order. error: {0}", error);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    foreach (var submitedOrder in submittedOrders)
                    {
                        if (submitedOrder.OrderNo == order.OrderNo && TradingHelper.IsFinalStatus(submitedOrder.Status))
                        {
                            return true;
                        }
                    }

                    Thread.Sleep(1000);
                } while (true);
            }

            return false;
        }

        public bool[] CancelOrder(DispatchedOrder[] orders, out string[] errors, bool waitForResult)
        {
            var codes = orders.Select(o => o.Request.SecurityCode).ToArray();
            var orderNos = orders.Select(o => o.OrderNo).ToArray();

            bool[] succeededFlags = _client.CancelOrder(codes, orderNos, out errors);

            if (!waitForResult)
            {
                return succeededFlags;
            }

            bool[] finalSucceededFlags = new bool[succeededFlags.Length];
            Array.Clear(finalSucceededFlags, 0, finalSucceededFlags.Length);

            do
            {
                string error;
                var submittedOrders = _client.QuerySubmittedOrderToday(out error).ToList();

                if (submittedOrders.Count() == 0)
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        AppLogger.Default.WarnFormat("Failed to query submitted order. error: {0}", error);
                        
                        for (int i = 0; i < succeededFlags.Length; ++i)
                        {
                            if (succeededFlags[i] && !finalSucceededFlags[i])
                            {
                                succeededFlags[i] = false;
                                finalSucceededFlags[i] = false;
                                errors[i] = error;
                            }
                        }
                    }

                    return finalSucceededFlags;
                }

                foreach (var submitedOrder in submittedOrders)
                {
                    for (int i = 0; i < succeededFlags.Length; ++i)
                    {
                        if (succeededFlags[i] && !finalSucceededFlags[i])
                        {
                            if (submitedOrder.OrderNo == orders[i].OrderNo && TradingHelper.IsFinalStatus(submitedOrder.Status))
                            {
                                finalSucceededFlags[i] = true;
                                break;
                            }
                        }
                    }
                }

                int pendingOrderCount = Enumerable
                    .Range(0, succeededFlags.Length)
                    .Count(i => succeededFlags[i] && !finalSucceededFlags[i]);

                if (pendingOrderCount == 0)
                {
                    break;
                }

                Thread.Sleep(1000);
            } while (true);

            return finalSucceededFlags;
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

                        // remove order in finished status
                        if (TradingHelper.IsFinalStatus(order.Status))
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

            if (orderResult.Status != dispatchedOrder.LastStatus)
            {
                isStatusChanged = true;
            }
            else if (orderResult.Status == OrderStatus.PartiallySucceeded 
                && dispatchedOrder.LastStatus == orderResult.Status
                && orderResult.DealVolume != dispatchedOrder.LastDealVolume)
            {
                isStatusChanged = true;
            }

            dispatchedOrder.LastStatus = orderResult.Status;
            dispatchedOrder.LastDealVolume = orderResult.DealVolume;
            dispatchedOrder.LastDealPrice = orderResult.DealPrice;

            if (isStatusChanged)
            {
                NotifyOrderStatusChanged(dispatchedOrder);
            }

            return isStatusChanged;
        }

        private void NotifyOrderStatusChanged(DispatchedOrder dispatchedOrder)
        {
            if (_onOrderStatusChangedCallback != null)
            {
                _onOrderStatusChangedCallback(dispatchedOrder);
            }
        }
    }
}
