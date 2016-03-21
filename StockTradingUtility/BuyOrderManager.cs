using System;
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
    public sealed class BuyOrderManager
    {
        public const int MinCancellationIntervalInMillisecond = 1000;

        private static BuyOrderManager _instance = null;

        private object _orderLockObj = new object();

        private IDictionary<string, List<BuyOrder>> _activeOrders = new Dictionary<string, List<BuyOrder>>();

        private HashSet<DispatchedOrder> _dispatchedOrders = new HashSet<DispatchedOrder>();

        public delegate void OnOrderExecutedDelegate(BuyOrder order, float dealPrice, int dealVolume);

        public OnOrderExecutedDelegate OnBuyOrderExecuted { get; set; }

        private Timer _cancelOrderTimer = null;

        private int _cancellationIntervalInMillisecond = MinCancellationIntervalInMillisecond;

        public int CancallationIntervalInMillisecond
        {
            get { return _cancellationIntervalInMillisecond;  }
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

        public static BuyOrderManager GetInstance()
        {
            if (_instance == null)
            {
                lock (typeof(BuyOrderManager))
                {
                    if (_instance == null)
                    {
                        _instance = new BuyOrderManager();
                    }
                }
            }

            return _instance;
        }

        private BuyOrderManager()
        {
            CtpSimulator.GetInstance().RegisterQuoteReadyCallback(OnQuoteReady);
            CtpSimulator.GetInstance().RegisterOrderStatusChangedCallback(OnOrderStatusChanged);

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
                    try
                    {
                        if ((now - dispatchedOrder.DispatchTime).TotalMilliseconds >= _cancellationIntervalInMillisecond)
                        {
                            string error;

                            if (!CtpSimulator.GetInstance().CancelOrder(dispatchedOrder, out error))
                            {
                                ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                                if (logger != null)
                                {
                                    BuyOrder order = (BuyOrder)dispatchedOrder.Request.AssociatedObject;

                                    logger.ErrorFormat(
                                        "fail to cancel buy order: order no {0} id {1} code {2} expected price {3}, volume {4}. Error: {5}",
                                        dispatchedOrder.OrderNo,
                                        order.OrderId,
                                        order.SecurityCode,
                                        order.ExpectedPrice,
                                        order.RemainingVolumeCanBeBought,
                                        error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                        if (logger != null)
                        {
                            BuyOrder order = (BuyOrder)dispatchedOrder.Request.AssociatedObject;

                            logger.ErrorFormat(
                                "Exception in cancelling buy order: order no {0} id {1} code {2} expected price {3}, volume {4}. Error: {5}",
                                dispatchedOrder.OrderNo,
                                order.OrderId,
                                order.SecurityCode,
                                order.ExpectedPrice,
                                order.RemainingVolumeCanBeBought,
                                ex.ToString());
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(_orderLockObj);
            }
        }

        private bool ShouldBuy(FiveLevelQuote quote, float maxSellPrice, float minSellPrice, int totalSellVolume, BuyOrder order)
        {
            bool shouldBuy = false;

            if (order.ExpectedPrice >= minSellPrice)
            {
                shouldBuy = true;
            }

            return shouldBuy;
        }

        private void OnQuoteReady(FiveLevelQuote[] quotes, string[] errors)
        {
            if (quotes == null || quotes.Length == 0)
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

            for (int i = 0; i < quotes.Length; ++i)
            {
                var quote = quotes[i];

                if (quote == null)
                {
                    continue;
                }

                lock (_orderLockObj)
                {
                    List<BuyOrder> orders;
                    if (!_activeOrders.TryGetValue(quote.SecurityCode, out orders))
                    {
                        continue;
                    }
                    
                    // copy orders to avoid the "orders" object being updated in SendBuyOrder function.
                    var OrderCopies = orders.ToArray();

                    float maxSellPrice = quote.SellPrices.Max();
                    float minSellPrice = quote.SellPrices.Min();
                    int totalSellVolume = ChinaStockHelper.ConvertHandToVolume(quote.SellVolumesInHand.Sum());

                    foreach (var order in OrderCopies)
                    {
                        if (ShouldBuy(quote, maxSellPrice, minSellPrice, totalSellVolume, order))
                        {
                            SendBuyOrder(order);
                        }
                    }
                }
            }
        }

        private void SendBuyOrder(BuyOrder order)
        {
            // put it into thread pool to avoid recursively call QueryOrderStatusForcibly and 
            // then call OnOrderStatusChanged() and then call SendBuyOrder recursively.
            ThreadPool.QueueUserWorkItem(SendBuyOrderWorkItem, order);
        }

        private void SendBuyOrderWorkItem(object state)
        {
            BuyOrder order = (BuyOrder)state;

            OrderRequest request = new OrderRequest(order)
            {
                Category = OrderCategory.Buy,
                Price = order.MaxBidPrice,
                PricingType = OrderPricingType.LimitPrice,
                Volume = order.GetMaxVolumeInHandToBuy(order.MaxBidPrice),
                SecurityCode = order.SecurityCode,
            };

            string error;
            DispatchedOrder dispatchedOrder = CtpSimulator.GetInstance().DispatchOrder(request, out error);

            if (dispatchedOrder == null)
            {
                ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                if (logger != null)
                {
                    logger.ErrorFormat(
                        "Exception in dispatching buy order: id {0} code {1} expected price {2}, volume {3}. Error: {4}",
                        order.OrderId,
                        order.SecurityCode,
                        order.ExpectedPrice,
                        order.RemainingVolumeCanBeBought,
                        error);
                }
            }
            else
            {
                ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                if (logger != null)
                {
                    logger.InfoFormat(
                        "Dispatched buy order: id {0} code {1} expected price {2}, volume {3}.",
                        order.OrderId,
                        order.SecurityCode,
                        order.ExpectedPrice,
                        order.RemainingVolumeCanBeBought);
                }

                lock (_orderLockObj)
                {
                    RemoveActiveBuyOrder(order);
                    AddDispatchedOrder(dispatchedOrder);
                }

                // force query order status.
                CtpSimulator.GetInstance().QueryOrderStatusForcibly();
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
                if (!IsDispatchedOrder(dispatchedOrder))
                {
                    return;
                }
            }

            if (dispatchedOrder.Request.AssociatedObject != null)
            {
                BuyOrder order = dispatchedOrder.Request.AssociatedObject as BuyOrder;

                if (order == null)
                {
                    return;
                }

                if (TradingHelper.IsFinalStatus(dispatchedOrder.LastStatus))
                {
                    ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                    if (logger != null)
                    {
                        logger.InfoFormat(
                            "Buy order executed:  id {0} code {1} status {2} succeeded volume {3} deal price {4}.",
                            order.OrderId,
                            order.SecurityCode,
                            dispatchedOrder.LastStatus,
                            dispatchedOrder.LastDealVolume,
                            dispatchedOrder.LastDealPrice);
                    }

                    if (dispatchedOrder.LastDealVolume > 0)
                    {
                        order.Fulfill(dispatchedOrder.LastDealPrice, dispatchedOrder.LastDealVolume);

                        // callback to client to notify partial or full success
                        if (OnBuyOrderExecuted != null)
                        {
                            OnBuyOrderExecuted(order, dispatchedOrder.LastDealPrice, dispatchedOrder.LastDealVolume);
                        }
                    }

                    RemoveDispatchedOrder(dispatchedOrder);

                    if (!order.IsCompleted(order.ExpectedPrice))
                    {
                        // the order has not been finished yet, put it back into active order
                        AddActiveBuyOrder(order);
                    }
                }
            }
        }

        public void RegisterBuyOrder(BuyOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            lock (_orderLockObj)
            {
                AddActiveBuyOrder(order);
            }
        }

        public bool UnregisterBuyOrder(BuyOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            lock (_orderLockObj)
            {
                return RemoveActiveBuyOrder(order);
            }
        }

        private void AddActiveBuyOrder(BuyOrder order)
        {
            if (!_activeOrders.ContainsKey(order.SecurityCode))
            {
                _activeOrders.Add(order.SecurityCode, new List<BuyOrder>());

                CtpSimulator.GetInstance().SubscribeQuote(order.SecurityCode);
            }

            _activeOrders[order.SecurityCode].Add(order);
        }

        private bool RemoveActiveBuyOrder(BuyOrder order)
        {
            List<BuyOrder> orders;
            if (!_activeOrders.TryGetValue(order.SecurityCode, out orders))
            {
                return false;
            }

            bool removeSucceeded = orders.Remove(order);

            if (orders.Count == 0)
            {
                _activeOrders.Remove(order.SecurityCode);

                CtpSimulator.GetInstance().UnsubscribeQuote(order.SecurityCode);
            }

            return removeSucceeded;
        }

        private void AddDispatchedOrder(DispatchedOrder order)
        {
            _dispatchedOrders.Add(order);
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
