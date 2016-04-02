﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using StockAnalysis.Share;

using log4net;

namespace StockTrading.Utility
{
    public sealed class OrderManager
    {
        public const int MinCancellationIntervalInMillisecond = 1000;

        private static OrderManager _instance = null;

        private object _orderLockObj = new object();

        private IDictionary<string, List<IOrder>> _activeOrders = new Dictionary<string, List<IOrder>>();

        private HashSet<DispatchedOrder> _dispatchedOrders = new HashSet<DispatchedOrder>();

        private Timer _cancelOrderTimer = null;

        private int _cancellationIntervalInMillisecond = MinCancellationIntervalInMillisecond;

        public int CancallationIntervalInMillisecond
        {
            get { return _cancellationIntervalInMillisecond; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Cancellation interval must be greater than 0");
                }

                _cancelOrderTimer.Change(value, value);
                _cancellationIntervalInMillisecond = value;
            }
        }

        public static OrderManager GetInstance()
        {
            if (_instance == null)
            {
                lock (typeof(OrderManager))
                {
                    if (_instance == null)
                    {
                        _instance = new OrderManager();
                    }
                }
            }

            return _instance;
        }

        private OrderManager()
        {
            _cancelOrderTimer = new Timer(CancelOrderTimerCallback, null, _cancellationIntervalInMillisecond, _cancellationIntervalInMillisecond);
        }

        private void CancelOrderTimerCallback(object obj)
        {
            if (!Monitor.TryEnter(_orderLockObj))
            {
                return;
            }

            try
            {
                DateTime now = DateTime.Now;

                foreach (var dispatchedOrder in _dispatchedOrders)
                {
                    IOrder order = (IOrder)dispatchedOrder.Request.AssociatedObject;

                    if (!order.ShouldCancelIfNotSucceeded)
                    {
                        continue;
                    }

                    try
                    {
                        if ((now - dispatchedOrder.DispatchedTime).TotalMilliseconds >= _cancellationIntervalInMillisecond)
                        {
                            string error;

                            if (!CtpSimulator.GetInstance().CancelOrder(dispatchedOrder, out error, false))
                            {
                                AppLogger.Default.ErrorFormat(
                                    "fail to cancel order: order no {0}. order details: {1}, Error: {2}",
                                    dispatchedOrder.OrderNo,
                                    order,
                                    error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Default.ErrorFormat(
                            "Exception in cancelling buy order: order no {0}. order details: {1}. Error: {2}",
                            dispatchedOrder.OrderNo,
                            order,
                            ex.ToString());
                    }
                }
            }
            finally
            {
                Monitor.Exit(_orderLockObj);
            }
        }

        private void OnQuoteReady(IEnumerable<QuoteResult> quotes)
        {
            if (quotes == null || quotes.Count() == 0)
            {
                return;
            }

            lock (_orderLockObj)
            {
                if (_activeOrders.Count == 0)
                {
                    return;
                }
            }

            foreach (var quote in quotes)
            {
                if (!quote.IsValidQuote())
                {
                    continue;
                }

                IOrder[] orderCopies = null;

                lock (_orderLockObj)
                {
                    List<IOrder> orders;
                    if (!_activeOrders.TryGetValue(quote.SecurityCode, out orders))
                    {
                        continue;
                    }

                    // copy orders to avoid the "orders" object being updated in SendOrder function.
                    orderCopies = orders.ToArray();

                    // we need to keep this part in the lock to avoid the order was unregistered
                    // before being sent out.
                    foreach (var order in orderCopies)
                    {
                        if (order.ShouldExecute(quote.Quote))
                        {
                            SendOrder(order, quote.Quote);
                        }
                    }
                }
            }
        }

        private void SendOrder(IOrder order, FiveLevelQuote quote)
        {
            OrderRequest request = order.BuildRequest(quote);

            string error;

            // we must add lock here to avoid an order being created and executed immediately, 
            // and then OnOrderStatusChanged will be called and cause problem.
            lock (_orderLockObj)
            {
                DispatchedOrder dispatchedOrder
                    = CtpSimulator.GetInstance().DispatchOrder(request, OnOrderStatusChanged, out error);

                if (dispatchedOrder == null)
                {
                    AppLogger.Default.ErrorFormat(
                        "Exception in dispatching order, Error: {0}. Order details: {1}",
                        error,
                        order);
                }
                else
                {
                    AppLogger.Default.InfoFormat("Dispatched order. Order details: {0}.", order);

                    // reset the event after dispatched
                    order.NoActiveOrderDispatched.Reset();

                    RemoveActiveOrder(order, false);
                    if (!AddDispatchedOrder(dispatchedOrder))
                    {
                        throw new InvalidOperationException(
                            string.Format("Failed to add dispatched order. Order details: {0}", order));
                    }
                }
            }
        }

        private void OnOrderStatusChanged(DispatchedOrder dispatchedOrder)
        {
            if (dispatchedOrder == null)
            {
                return;
            }

            lock (_orderLockObj)
            {
                // it is possible we can't find the order in _dispatchedOrders because it has 
                // been unregistered
                bool isCancelled = !IsDispatchedOrder(dispatchedOrder);

                if (dispatchedOrder.Request.AssociatedObject != null)
                {
                    IOrder order = (IOrder)dispatchedOrder.Request.AssociatedObject;

                    if (TradingHelper.IsFinalStatus(dispatchedOrder.LastStatus))
                    {
                        AppLogger.Default.InfoFormat(
                             "Order executed:  id {0} code {1} status {2} deal volume {3} average deal price {4}.",
                             order.OrderId,
                             order.SecurityCode,
                             dispatchedOrder.LastStatus,
                             dispatchedOrder.LastTotalDealVolume,
                             dispatchedOrder.LastAverageDealPrice);

                        if (dispatchedOrder.LastTotalDealVolume > 0)
                        {
                            order.Deal(dispatchedOrder.LastAverageDealPrice, dispatchedOrder.LastTotalDealVolume);
                        }

                        // callback to client to notify partial or full success
                        if (order.OnOrderExecuted != null)
                        {
                            // use sync call here although there is risk of deadlock, to ensure the order's event
                            // can be set properly without worrying if the callback has been executed.
                            order.OnOrderExecuted(order, dispatchedOrder.LastAverageDealPrice, dispatchedOrder.LastTotalDealVolume);
                        }

                        // set event after dispatched order has been executed.
                        order.NoActiveOrderDispatched.Set();

                        if (!isCancelled)
                        {
                            if (!RemoveDispatchedOrder(dispatchedOrder))
                            {
                                AppLogger.Default.ErrorFormat("Failed to remove dispatched order. Order details: {0}", order);
                            }

                            if (!order.IsCompleted())
                            {
                                // the order has not been finished yet, put it back into active order
                                AddActiveOrder(order);
                            }
                        }
                    }
                }
            }
        }

        public void RegisterOrder(IOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            lock (_orderLockObj)
            {
                AddActiveOrder(order);
            }
        }

        public bool UnregisterOrder(IOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            lock (_orderLockObj)
            {
                return RemoveActiveOrder(order, true);
            }
        }

        private void AddActiveOrder(IOrder order)
        {
            if (!_activeOrders.ContainsKey(order.SecurityCode))
            {
                _activeOrders.Add(order.SecurityCode, new List<IOrder>());

                CtpSimulator.GetInstance().SubscribeQuote(new QuoteSubscription(order.SecurityCode, OnQuoteReady));
            }

            _activeOrders[order.SecurityCode].Add(order);
        }

        private bool RemoveActiveOrder(IOrder order, bool shouldCancelOrder)
        {
            List<IOrder> orders;
            bool removeSucceeded = false;
            if (_activeOrders.TryGetValue(order.SecurityCode, out orders))
            {
                removeSucceeded = orders.Remove(order);

                if (orders.Count == 0)
                {
                    _activeOrders.Remove(order.SecurityCode);

                    CtpSimulator.GetInstance().UnsubscribeQuote(new QuoteSubscription(order.SecurityCode, OnQuoteReady));
                }
            }

            if (!removeSucceeded && shouldCancelOrder)
            {
                // maybe the order has been dispatched, we need to cancel it
                var dispatchedOrder = _dispatchedOrders.FirstOrDefault(o => object.ReferenceEquals(o.Request.AssociatedObject, order));

                string error;
                if (dispatchedOrder != null)
                {
                    if (!CtpSimulator.GetInstance().CancelOrder(dispatchedOrder, out error, true))
                    {
                        AppLogger.Default.ErrorFormat(
                            "Cancel dispatched stoploss order failed. Error: {0}. Order details: {1}",
                            error,
                            order);

                        removeSucceeded = false;
                    }
                    else
                    {
                        if (!RemoveDispatchedOrder(dispatchedOrder))
                        {
                            AppLogger.Default.ErrorFormat("Failed to remove dispatched order. Order details: {0}", dispatchedOrder.Request.AssociatedObject);
                        }

                        removeSucceeded = true;
                    }
                }
                else
                {
                    removeSucceeded = true;
                }
            }

            return removeSucceeded;
        }

        private bool AddDispatchedOrder(DispatchedOrder order)
        {
            return _dispatchedOrders.Add(order);
        }

        private bool RemoveDispatchedOrder(DispatchedOrder order)
        {
            return _dispatchedOrders.Remove(order);
        }

        private bool IsDispatchedOrder(DispatchedOrder order)
        {
            return _dispatchedOrders.Contains(order);
        }
    }
}