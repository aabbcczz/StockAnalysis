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
                OrderNo = result.OrderNo,
                Request = request,
                SucceededVolume = 0,
                LastStatus = OrderStatus.NotSubmitted,
            };

            lock (_orderLockObj)
            {
                _allActiveOrders.Add(result.OrderNo, dispatchedOrder);
            }

            return dispatchedOrder;
        }

        public bool CancelOrder(DispatchedOrder order, out string error)
        {
            bool isCancelled = _client.CancelOrder(order.Request.SecurityCode, order.OrderNo, out error);

            return isCancelled;
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

            if (_isStopped)
            {
                return;
            }

            if (_client == null || !_client.IsLoggedOn())
            {
                return;
            }

            try
            {
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
                        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                        if (logger != null)
                        {
                            logger.WarnFormat("Failed to query cancellable order. error: {0}", error);
                        }
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
                        if (TradingHelper.IsFinishedStatus(order.Status))
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
                ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                if (logger != null)
                {
                    logger.ErrorFormat("Exception in querying order status: {0}", ex);
                }
            }
            finally
            {
                Monitor.Exit(_dispatcherLockObj);
            }
        }

        private bool CheckOrderStatusChangeAndNotify(ref DispatchedOrder dispatchedOrder, QueryGeneralOrderResult order)
        {
            bool isStatusChanged = false;

            if (order.Status != dispatchedOrder.LastStatus)
            {
                isStatusChanged = true;
            }
            else if (order.Status == OrderStatus.PartiallySucceeded 
                && dispatchedOrder.LastStatus == order.Status
                && order.DealVolume != dispatchedOrder.SucceededVolume)
            {
                isStatusChanged = true;
            }

            dispatchedOrder.LastStatus = order.Status;
            dispatchedOrder.SucceededVolume = order.DealVolume;

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
