using System.Collections.Generic;
using StockAnalysis.Common.Utility;

namespace StockAnalysis.StockTrading.Utility
{
    public sealed class CtpSimulator
    {
        /// <summary>
        /// the quote refreshing interval in millisecond.
        /// </summary>
        public const int QuoteRefreshingInterval = 3000;

        /// <summary>
        /// the order refreshing interval in millisecond.
        /// </summary>
        public const int OrderRefreshingInterval = 1000;

        private static CtpSimulator _instance = null;

        private object _initializationLockObj = new object();

        private TradingClient _client = null;
        private bool _initialized = false;

        private QuotePublisher _quotePublisher = null;
        private OrderDispatcher _orderDispatcher = null;

        public static CtpSimulator GetInstance()
        {
            if (_instance == null)
            {
                lock (typeof(CtpSimulator))
                {
                    if (_instance == null)
                    {
                        _instance = new CtpSimulator();
                    }
                }
            }

            return _instance;
        }

        private CtpSimulator()
        {
        }

        public bool Initialize(
            bool useSimulatorServer,
            bool enableSinaQuote,
            string address, 
            short port, 
            string protocolVersion, 
            short yybId, 
            string accountNo,
            short accountType,
            string tradeAccount, 
            string tradePassword, 
            string communicationPassword,
            out string error)
        {
            if (!_initialized)
            {
                lock (_initializationLockObj)
                {
                    if (!_initialized)
                    {
                        TradingEnvironment.Initialize();

                        _client = new TradingClient(TradingHelper.CreateTradingServer(useSimulatorServer));

                        if (!_client.LogOn(address, port, protocolVersion, yybId, accountNo, accountType, tradeAccount, tradePassword, communicationPassword, out error))
                        {
                            _client.Dispose();
                            _client = null;

                            TradingEnvironment.UnInitialize();

                            return false;
                        }

                        _quotePublisher = new QuotePublisher(_client, QuoteRefreshingInterval, enableSinaQuote);
                        _orderDispatcher = new OrderDispatcher(_client, OrderRefreshingInterval);

                        _initialized = true;

                        return true;
                    }
                }
            }

            error = "CtpSimulator has been initialized already, can't be reinitialized";
            return false;
        }

        public void UnInitialize()
        {
            if (_initialized)
            {
                lock (_initializationLockObj)
                {
                    if (_initialized)
                    {
                        _quotePublisher.Stop();
                        _quotePublisher = null;

                        _orderDispatcher.Stop();
                        _orderDispatcher = null;

                        _client.LogOff();
                        _client = null;

                        TradingEnvironment.UnInitialize();

                        _initialized = false;
                    }
                }
            }
        }

        public void SubscribeQuote(QuoteSubscription subscription)
        {
            _quotePublisher.Subscribe(subscription);
        }

        public void SubscribeQuote(IEnumerable<QuoteSubscription> subscriptions)
        {
            _quotePublisher.Subscribe(subscriptions);
        }

        public void UnsubscribeQuote(QuoteSubscription subscription)
        {
            _quotePublisher.Unsubscribe(subscription);
        }

        public void UnsubscribeQuote(IEnumerable<QuoteSubscription> subscriptions)
        {
            _quotePublisher.Unsubscribe(subscriptions);
        }

        public DispatchedOrder DispatchOrder(OrderRequest request, WaitableConcurrentQueue<OrderStatusChangedMessage> orderStatusChangedMessageReceiver, out string error)
        {
            return _orderDispatcher.DispatchOrder(request, orderStatusChangedMessageReceiver, out error);
        }

        public bool CancelOrder(DispatchedOrder order, out string error)
        {
            return _orderDispatcher.CancelOrder(order, out error);
        }

        public void QueryOrderStatusForcibly()
        {
            _orderDispatcher.QueryOrderStatusForcibly();
        }

        public QueryCapitalResult QueryCapital(out string error)
        {
            return _client.QueryCapital(out error);
        }
    }
}
