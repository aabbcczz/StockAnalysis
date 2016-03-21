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
    public sealed class StoplossOrderManager
    {
        private static StoplossOrderManager _instance = null;

        private object _orderLockObj = new object();

        private IDictionary<string, List<StoplossOrder>> _activeOrders = new Dictionary<string, List<StoplossOrder>>();

        private HashSet<DispatchedOrder> _dispatchedOrders = new HashSet<DispatchedOrder>();

        public delegate void OnStoplossOrderExecutedDelegate(StoplossOrder order, float dealPrice, int dealVolume);

        public OnStoplossOrderExecutedDelegate OnStoplossOrderExecuted { get; set; }

        public static StoplossOrderManager GetInstance()
        {
            if (_instance == null)
            {
                lock (typeof(StoplossOrderManager))
                {
                    if (_instance == null)
                    {
                        _instance = new StoplossOrderManager();
                    }
                }
            }

            return _instance;
        }

        private StoplossOrderManager()
        {
            CtpSimulator.GetInstance().RegisterQuoteReadyCallback(OnQuoteReady);
            CtpSimulator.GetInstance().RegisterOrderStatusChangedCallback(OnOrderStatusChanged);
        }

        private bool ShouldStoploss(FiveLevelQuote quote, float maxBuyPrice, float minBuyPrice, int totalBuyVolume, StoplossOrder order)
        {
            bool shouldStoploss = false;

            if (order.StoplossPrice < minBuyPrice)
            {
                if (totalBuyVolume > order.RemainingVolume)
                {
                    // don't worry
                }
                else
                {
                    if (maxBuyPrice <= minBuyPrice)
                    {
                        // limit down. do nothing.
                    }
                    else
                    {
                        // predicate volume by using linear extrapolation
                        int predicateVolume = (int)((double)totalBuyVolume * (maxBuyPrice - order.StoplossPrice) / (maxBuyPrice - minBuyPrice));

                        if (predicateVolume <= order.RemainingVolume * 2)
                        {
                            shouldStoploss = true;
                        }
                    }
                }
            }
            else
            {
                if (order.StoplossPrice >= maxBuyPrice)
                {
                    // need to stop loss immediately.
                    shouldStoploss = true;
                }
                else
                {
                    // order stop loss price is between minBuyPrice and maxBuyPrice
                    // we count the buy volume above stop loss price.
                    int aboveStoplossBuyVolume =
                        ChinaStockHelper.ConvertHandToVolume(
                            Enumerable
                                .Range(0, quote.BuyPrices.Length)
                                .Where(index => quote.BuyPrices[index] >= order.StoplossPrice)
                                .Sum(index => quote.BuyVolumesInHand[index]));

                    if (aboveStoplossBuyVolume <= order.RemainingVolume * 3)
                    {
                        shouldStoploss = true;
                    }
                    else
                    {
                        // there is enough buy volume now, so don't need to be rush.
                    }
                }
            }

            return shouldStoploss;
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
                    List<StoplossOrder> orders;
                    if (!_activeOrders.TryGetValue(quote.SecurityCode, out orders))
                    {
                        continue;
                    }
                    
                    // copy orders to avoid the "orders" object being updated in SendStoplossOrder function.
                    var OrderCopies = orders.ToArray();

                    float maxBuyPrice = quote.BuyPrices.Max();
                    float minBuyPrice = quote.BuyPrices.Min();
                    int totalBuyVolume = ChinaStockHelper.ConvertHandToVolume(quote.BuyVolumesInHand.Sum());

                    foreach (var order in OrderCopies)
                    {
                        if (ShouldStoploss(quote, maxBuyPrice, minBuyPrice, totalBuyVolume, order))
                        {
                            SendStoplossOrder(order);
                        }
                    }
                }
            }
        }

        private void SendStoplossOrder(StoplossOrder order)
        {
            // put it into thread pool to avoid recursively call QueryOrderStatusForcibly and 
            // then call OnOrderStatusChanged() and then call SendStoplossOrder recursively.
            ThreadPool.QueueUserWorkItem(SendStoplossOrderWorkItem, order);
        }

        private void SendStoplossOrderWorkItem(object state)
        {
            StoplossOrder order = (StoplossOrder)state;

            OrderRequest request = new OrderRequest(order)
            {
                Category = OrderCategory.Sell,
                Price = order.StoplossPrice,
                PricingType = OrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                Volume = order.RemainingVolume,
                SecurityCode = order.SecurityCode,
            };

            string error;
            DispatchedOrder dispatchedOrder = CtpSimulator.GetInstance().DispatchOrder(request, out error);

            if (dispatchedOrder == null)
            {

                AppLogger.Default.ErrorFormat(
                    "Exception in dispatching stop loss order: id {0} code {1} stoploss price {2}, volume {3}. Error: {4}",
                    order.OrderId,
                    order.SecurityCode,
                    order.StoplossPrice,
                    order.RemainingVolume,
                    error);

            }
            else
            {

                AppLogger.Default.InfoFormat(
                    "Dispatched stop loss order: id {0} code {1} stoploss price {2}, volume {3}.",
                    order.OrderId,
                    order.SecurityCode,
                    order.StoplossPrice,
                    order.RemainingVolume);

                lock (_orderLockObj)
                {
                    RemoveActiveStoplossOrder(order);
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
                StoplossOrder order = dispatchedOrder.Request.AssociatedObject as StoplossOrder;

                if (order == null)
                {
                    return;
                }

                if (TradingHelper.IsFinalStatus(dispatchedOrder.LastStatus))
                {
                    lock (_orderLockObj)
                    {
                        AppLogger.Default.InfoFormat(
                            "Stoploss order executed:  id {0} code {1} status {2} succeeded volume {3} deal price {4}.",
                            order.OrderId,
                            order.SecurityCode,
                            dispatchedOrder.LastStatus,
                            dispatchedOrder.LastDealVolume,
                            dispatchedOrder.LastDealPrice);

                        if (dispatchedOrder.LastDealVolume > 0)
                        {
                            order.Fulfill(dispatchedOrder.LastDealPrice, dispatchedOrder.LastDealVolume);

                            // callback to client to notify partial or full success
                            if (OnStoplossOrderExecuted != null)
                            {
                                OnStoplossOrderExecuted(order, dispatchedOrder.LastDealPrice, dispatchedOrder.LastDealVolume);
                            }
                        }

                        RemoveDispatchedOrder(dispatchedOrder);

                        if (!order.IsCompleted())
                        {
                            // the order has not been finished yet, put it back into active order
                            AddActiveStoplossOrder(order);

                            if (TradingHelper.IsSucceededFinalStatus(dispatchedOrder.LastStatus))
                            {
                                // send out order again
                                SendStoplossOrder(order);
                            }
                        }
                    }
                }
            }
        }

        public void RegisterStoplossOrder(StoplossOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            lock (_orderLockObj)
            {
                AddActiveStoplossOrder(order);
            }
        }

        public bool UnregisterStoplossOrder(StoplossOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException();
            }

            lock (_orderLockObj)
            {
                return RemoveActiveStoplossOrder(order);
            }
        }

        private void AddActiveStoplossOrder(StoplossOrder order)
        {
            if (!_activeOrders.ContainsKey(order.SecurityCode))
            {
                _activeOrders.Add(order.SecurityCode, new List<StoplossOrder>());

                CtpSimulator.GetInstance().SubscribeQuote(order.SecurityCode);
            }

            _activeOrders[order.SecurityCode].Add(order);
        }

        private bool RemoveActiveStoplossOrder(StoplossOrder order)
        {
            List<StoplossOrder> orders;
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
