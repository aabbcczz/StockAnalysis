using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockTrading.Utility;
using StockAnalysis.Share;
using System.Threading;

namespace TradingClient
{
    sealed class StrategyGdbExecuter
    {
        private const int MaxNumberOfNewStockCanBeBoughtInOneDay = 6; // 策略一日内允许买入的股票最大数目

        private const string DataFileFolder = "StrategyGDB";
        private readonly TimeSpan _startRunTime = new TimeSpan(9, 29, 0);
        private readonly TimeSpan _endRunTime = new TimeSpan(14, 50, 0);
        private readonly TimeSpan _startExecuteTime = new TimeSpan(9, 29, 0);
        private readonly TimeSpan _endExecuteTime = new TimeSpan(15, 30, 0);
        private readonly TimeSpan _startAcceptQuoteTime = new TimeSpan(9, 30, 0);
        private readonly TimeSpan _endAcceptQuoteTime = new TimeSpan(14, 56, 0);
        private readonly TimeSpan _startPublishStoplossOrderTime = new TimeSpan(9, 30, 0);
        private readonly TimeSpan _endPublishStoplossOrderTime = new TimeSpan(14, 56, 0);
        private readonly TimeSpan _startPublishBuyOrderTime = new TimeSpan(9, 30, 0);
        private readonly TimeSpan _endPublishBuyOrderTime = new TimeSpan(14, 55, 0);
        private readonly TimeSpan _startPublishSellOrderTime = new TimeSpan(14, 55, 0);
        private readonly TimeSpan _endPublishSellOrderTime = new TimeSpan(14, 57, 0);

        private WaitableConcurrentQueue<QuoteResult> _newStockQuotes = new WaitableConcurrentQueue<QuoteResult>();
        private WaitableConcurrentQueue<QuoteResult> _existingStockQuotes = new WaitableConcurrentQueue<QuoteResult>();
        private WaitableConcurrentQueue<OrderExecutedMessage> _buyOrderExecutedMessageReceiver = new WaitableConcurrentQueue<OrderExecutedMessage>();
        private WaitableConcurrentQueue<OrderExecutedMessage> _sellOrderExecutedMessageReceiver = new WaitableConcurrentQueue<OrderExecutedMessage>();
        private WaitableConcurrentQueue<OrderExecutedMessage> _stoplossOrderExecutedMessageReceiver = new WaitableConcurrentQueue<OrderExecutedMessage>();

        private CancellationToken _cancellationToken;

        // the new stock has been bought
        private HashSet<StrategyGDB.NewStock> _boughtStock = new HashSet<StrategyGDB.NewStock>();

        private List<StrategyGDB.NewStock> _newStocks = null;
        private List<StrategyGDB.ExistingStock> _existingStocks = null;
        
        private HashSet<string> _allSymbols = null;
        private Dictionary<string, StrategyGDB.NewStock> _activeNewStockIndex = null;
        private Dictionary<string, StrategyGDB.ExistingStock> _activeExistingStockIndex = null;

        private Dictionary<object, RuntimeStockOrder> _runtimeStockOrders = new Dictionary<object, RuntimeStockOrder>();

        private ReaderWriterLockSlim _runtimeReadWriteLock = new ReaderWriterLockSlim();

        private float _useableCapital = 0.0f;
        private object _queryCapitalLockObj = new object();

        public IEnumerable<RuntimeStockOrder> RuntimeStockOrders
        {
            get 
            {
                _runtimeReadWriteLock.EnterReadLock();

                try
                {
                    return new List<RuntimeStockOrder>(_runtimeStockOrders.Values);
                }
                finally
                {
                    _runtimeReadWriteLock.ExitReadLock();
                }
            }
        }

        public StrategyGdbExecuter(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
            {
                throw new ArgumentNullException();
            }

            _cancellationToken = cancellationToken;

            Initialize();
        }

        private void Initialize()
        {
            StrategyGDB.DataFileReaderWriter rw = new StrategyGDB.DataFileReaderWriter(DataFileFolder);

            rw.Read();

            _newStocks = rw.NewStocks.ToList();
            _existingStocks = rw.ExistingStocks.ToList();

            var allSymbols = _newStocks
                .Select(n => n.SecuritySymbol)
                .Union(_existingStocks.Select(e => e.SecuritySymbol))
                .Distinct();
                

            _allSymbols = new HashSet<string>(allSymbols);

            if (AppLogger.Default.IsDebugEnabled)
            {
                AppLogger.Default.DebugFormat(
                    "GDB strategy executer: loaded symbols {0}",
                    string.Join(",", _allSymbols));
            }

            _activeNewStockIndex = _newStocks.ToDictionary(s => s.SecuritySymbol, s => s);
            _activeExistingStockIndex = _existingStocks.ToDictionary(s => s.SecuritySymbol, s => s);
        }

        public void Run()
        {
            if (_allSymbols.Count == 0)
            {
                return;
            }

            if (!WaitForActionTime(_startRunTime, _endRunTime))
            {
                AppLogger.Default.ErrorFormat("Wait for valid trading time failed");
                return;
            }

            // update useable capital
            UpdateCurrentUseableCapital();

            // subscribe quote for all stocks
            CtpSimulator.GetInstance().SubscribeQuote(_newStocks.Select(s => new QuoteSubscription(s.SecuritySymbol, _newStockQuotes)));
            AppLogger.Default.InfoFormat(
                "Subscribe quote {0}", 
                string.Join(",",_newStocks.Select(s => s.SecuritySymbol)));

            CtpSimulator.GetInstance().SubscribeQuote(_existingStocks.Select(s => new QuoteSubscription(s.SecuritySymbol, _existingStockQuotes)));
            AppLogger.Default.InfoFormat(
                "Subscribe quote {0}",
                string.Join(",", _existingStocks.Select(s => s.SecuritySymbol)));

            // run an asynchronous task for listening quotes
            new Task(QuoteListener).Start();
            new Task(OrderExecutedListener).Start();

            while (IsValidActionTime(_startExecuteTime, _endExecuteTime))
            {
                Stoploss();
                Sell();

                Thread.Sleep(1000);
            }
        }

        private void QuoteListener()
        {
            WaitableConcurrentQueue<QuoteResult>[] queues = new WaitableConcurrentQueue<QuoteResult>[]
            {
                _newStockQuotes,
                _existingStockQuotes
            };

            int queueIndex;

            try
            {
                for (;;)
                {
                    var quote = WaitableConcurrentQueue<QuoteResult>.TakeFromAny(queues, _cancellationToken, out queueIndex);
                    if (quote != null)
                    {
                        if (queueIndex == 0)
                        {
                            OnNewStockQuoteReady(quote);
                        }
                        else if (queueIndex == 1)
                        {
                            OnExistingStockQuoteReady(quote);
                        }
                        else
                        {
                            throw new InvalidOperationException("queueIndex out of range. must be code bug");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                AppLogger.Default.ErrorFormat("Exception in listening quote: {0}", ex);
            }
        }

        private void OrderExecutedListener()
        {
            WaitableConcurrentQueue<OrderExecutedMessage>[] queues = new WaitableConcurrentQueue<OrderExecutedMessage>[]
            {
                _buyOrderExecutedMessageReceiver,
                _sellOrderExecutedMessageReceiver,
                _stoplossOrderExecutedMessageReceiver
            };

            int queueIndex;

            try
            {
                for (;;)
                {
                    var message = WaitableConcurrentQueue<OrderExecutedMessage>.TakeFromAny(queues, _cancellationToken, out queueIndex);
                    if (message != null)
                    {
                        if (queueIndex == 0)
                        {
                            OnBuyOrderExecuted(message);
                        }
                        else if (queueIndex == 1)
                        {
                            OnSellOrderExecuted(message);
                        }
                        else if (queueIndex == 2)
                        {
                            OnStoplossOrderExecuted(message);
                        }
                        else
                        {
                            throw new InvalidOperationException("queueIndex out of range. must be code bug");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                AppLogger.Default.ErrorFormat("Exception in listening quote: {0}", ex);
            }
        }


        private void UpdateCurrentUseableCapital()
        {
            lock (_queryCapitalLockObj)
            {
                string error;

                QueryCapitalResult result = CtpSimulator.GetInstance().QueryCapital(out error);

                float useableCapital = 0.0f;

                if (result == null)
                {
                    AppLogger.Default.ErrorFormat("Failed to query capital. Error: {0}", error);
                }
                else
                {
                    useableCapital = result.UsableCapital;
                }

                this._useableCapital = useableCapital;
            }
        }

        private bool WaitForActionTime(TimeSpan startTime, TimeSpan endTime)
        {
            do
            {
                TimeSpan now = DateTime.Now.TimeOfDay;

                if (now > endTime)
                {
                    return false;
                }

                if (now >= startTime && now <= endTime)
                {
                    return true;
                }

                System.Threading.Thread.Sleep(1000);
            } while (true);
        }

        private bool IsValidActionTime(TimeSpan startTime, TimeSpan endTime)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;

            return IsValidActionTime(now, startTime, endTime);
        }

        private bool IsValidActionTime(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
        {
            return (currentTime >= startTime && currentTime <= endTime);
        }

        private void UnsubscribeNewStock(string symbol)
        {
            CtpSimulator.GetInstance().UnsubscribeQuote(new QuoteSubscription(symbol, _newStockQuotes));
            AppLogger.Default.InfoFormat("Unsubscribe quote {0}", symbol);
        }

        private void UnsubscribeExistingStock(string symbol)
        {
            CtpSimulator.GetInstance().UnsubscribeQuote(new QuoteSubscription(symbol, _existingStockQuotes));
            AppLogger.Default.InfoFormat("Unsubscribe quote {0}", symbol);
        }

        private void Buy(FiveLevelQuote quote)
        {
            _runtimeReadWriteLock.EnterWriteLock();

            try
            {
                if (!IsValidActionTime(quote.Timestamp.TimeOfDay, _startAcceptQuoteTime, _endAcceptQuoteTime))
                {
                    return;
                }

                if (!_activeNewStockIndex.ContainsKey(quote.SecuritySymbol))
                {
                    UnsubscribeNewStock(quote.SecuritySymbol);
                    return;
                }

                var stock = _activeNewStockIndex[quote.SecuritySymbol];

                // check if buy order has been created
                if (_runtimeStockOrders.ContainsKey(stock))
                {
                    var runtime = _runtimeStockOrders[stock];
                    if (runtime.AssociatedBuyOrder != null)
                    {
                        return;
                    }
                }

                lock (_boughtStock)
                {
                    if (_boughtStock.Count >= StrategyGdbExecuter.MaxNumberOfNewStockCanBeBoughtInOneDay)
                    {
                        UnsubscribeNewStock(quote.SecuritySymbol);
                        return;
                    }
                }

                if (stock.DateToBuy.Date != DateTime.Today)
                {
                    // remove from active new stock
                    _activeNewStockIndex.Remove(quote.SecuritySymbol);

                    AppLogger.Default.WarnFormat(
                        "The buy date of stock {0:yyyy-MM-dd} is not today. {1}/{2}",
                        stock.DateToBuy,
                        stock.SecuritySymbol,
                        stock.SecurityName);

                    return;
                }

                // determine if open price is in valid range
                if (float.IsNaN(stock.ActualOpenPrice))
                {
                    double upLimitPrice = ChinaStockHelper.CalculatePrice(
                        quote.YesterdayClosePrice,
                        stock.OpenPriceUpLimitPercentage - 100.0f,
                        2);

                    double downLimitPrice = ChinaStockHelper.CalculatePrice(
                        quote.YesterdayClosePrice,
                        stock.OpenPriceDownLimitPercentage - 100.0f,
                        2);

                    if (quote.TodayOpenPrice < downLimitPrice
                        || quote.TodayOpenPrice > upLimitPrice
                        || quote.TodayOpenPrice < stock.StoplossPrice)
                    {
                        // remove from active new stock
                        _activeNewStockIndex.Remove(quote.SecuritySymbol);

                        AppLogger.Default.InfoFormat(
                            "Failed to buy stock because open price is out of range. {0}/{1} open {2:0.000} out of [{3:0.000}, {4:0.000}]",
                            stock.SecuritySymbol,
                            stock.SecurityName,
                            quote.TodayOpenPrice,
                            downLimitPrice,
                            upLimitPrice);
                    }
                    else
                    {
                        stock.ActualOpenPrice = quote.TodayOpenPrice;
                        stock.ActualOpenPriceDownLimit = (float)downLimitPrice;
                        stock.ActualOpenPriceUpLimit = (float)upLimitPrice;
                        stock.ActualMaxBuyPrice = (float)ChinaStockHelper.CalculatePrice(
                            quote.TodayOpenPrice,
                            stock.MaxBuyPriceIncreasePercentage,
                            2);
                        stock.TodayDownLimitPrice = (float)ChinaStockHelper.CalculateDownLimit(stock.SecuritySymbol, stock.SecurityName, quote.YesterdayClosePrice, 2);
                        stock.ActualMinBuyPrice = Math.Max(stock.StoplossPrice, stock.TodayDownLimitPrice);

                    }
                }

                // only buy those stock which has been raised over open price
                if (quote.CurrentPrice > quote.TodayOpenPrice)
                {
                    stock.IsBuyable = true;
                }

                if (stock.IsBuyable)
                {
                    if (IsValidActionTime(quote.Timestamp.TimeOfDay, _startPublishBuyOrderTime, _endPublishBuyOrderTime))
                    {
                        CreateBuyOrder(stock);

                        // unsubscribe the quote because all conditions has been setup.
                        UnsubscribeNewStock(quote.SecuritySymbol);
                    }
                }
            }
            finally
            {
                _runtimeReadWriteLock.ExitWriteLock();
            }
        }

        private void CreateBuyOrder(StrategyGDB.NewStock stock)
        {
            if (_runtimeStockOrders.ContainsKey(stock))
            {
                if (_runtimeStockOrders[stock].AssociatedBuyOrder != null)
                {
                    return;
                }
            }

            // update usable capital before issuing any buy order
            UpdateCurrentUseableCapital();

            if (_useableCapital < stock.TotalCapital * 0.9)
            {
                return;
            }

            float capital = Math.Min(stock.TotalCapital, _useableCapital);

            int maxVolume = (int)(capital / stock.ActualMaxBuyPrice);

            BuyInstruction instruction = new BuyInstruction(
                stock.SecuritySymbol,
                stock.SecurityName,
                stock.ActualMinBuyPrice,
                stock.ActualMaxBuyPrice,
                stock.ActualMaxBuyPrice,
                capital,
                maxVolume);

            BuyOrder order = new BuyOrder(instruction, _buyOrderExecutedMessageReceiver);

            if (_runtimeStockOrders.ContainsKey(stock))
            {
                _runtimeStockOrders[stock].AssociatedBuyOrder = order;
            }
            else
            {
                var runtime = new RuntimeStockOrder(stock.SecuritySymbol, stock.SecurityName)
                    {
                        ExpectedVolume = order.ExpectedVolume,
                        RemainingVolume = order.ExpectedVolume,
                        AssociatedBuyOrder = order,
                    };

                _runtimeStockOrders.Add(stock, runtime);
            }

            OrderManager.GetInstance().RegisterOrder(order);

            AppLogger.Default.InfoFormat("Registered order {0}", order);
        }

        private void OnBuyOrderExecuted(OrderExecutedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentException();
            }

            IOrder order = message.Order;
            int dealVolume = message.DealVolume;
            float dealPrice = message.DealPrice;

            AppLogger.Default.InfoFormat("Order executed. order details: {0}", order);

            if (dealVolume <= 0)
            {
                return;
            }

            HashSet<StrategyGDB.NewStock> boughtStockCopy = null;

            lock (_boughtStock)
            {
                if (_boughtStock.Count >= StrategyGdbExecuter.MaxNumberOfNewStockCanBeBoughtInOneDay)
                {
                    return;
                }
                 
                _boughtStock.Add(_activeNewStockIndex[order.SecuritySymbol]);

                if (_boughtStock.Count < StrategyGdbExecuter.MaxNumberOfNewStockCanBeBoughtInOneDay)
                {
                    return;
                }

                // create a copy to avoid deadlock between _boughtStock's lock and _runtimeReadWriteLock;
                boughtStockCopy = new HashSet<StrategyGDB.NewStock>(_boughtStock);
            }

            // remove all other buy orders which has not been executed successfully asynchronously
            // to avoid deadlock in OrderManager.
            Action action = () =>
            {
                _runtimeReadWriteLock.EnterWriteLock();

                try
                {
                    foreach (var kvp in _runtimeStockOrders)
                    {
                        if (kvp.Value.AssociatedBuyOrder != null)
                        {
                            if (!boughtStockCopy.Contains(kvp.Key))
                            {
                                OrderManager.GetInstance().UnregisterOrder(kvp.Value.AssociatedBuyOrder);
                                kvp.Value.AssociatedBuyOrder = null;
                            }
                        }
                    }
                }
                finally
                {
                    _runtimeReadWriteLock.ExitWriteLock();
                }
            };

            Task.Run(action);
        }

        private void Stoploss()
        {
            if (!IsValidActionTime(_startPublishStoplossOrderTime, _endPublishStoplossOrderTime))
            {
                return;
            }

            _runtimeReadWriteLock.EnterWriteLock();

            try
            {
                foreach (var stock in _activeExistingStockIndex.Values)
                {
                    if (!_runtimeStockOrders.ContainsKey(stock))
                    {
                        StoplossOrder order = new StoplossOrder(
                            stock.SecuritySymbol,
                            stock.SecurityName,
                            stock.StoplossPrice,
                            stock.Volume,
                            _stoplossOrderExecutedMessageReceiver);

                        RuntimeStockOrder runtime = new RuntimeStockOrder(stock.SecuritySymbol, stock.SecurityName)
                        {
                            AssociatedStoplossOrder = order,
                            ExpectedVolume = stock.Volume,
                            RemainingVolume = stock.Volume,
                        };

                        _runtimeStockOrders.Add(stock, runtime);

                        OrderManager.GetInstance().RegisterOrder(order);
                        AppLogger.Default.InfoFormat("Registered order {0}", order);
                    }
                    else
                    {
                        var runtime = _runtimeStockOrders[stock];
                        if (runtime.AssociatedSellOrder == null && runtime.AssociatedStoplossOrder == null)
                        {
                            StoplossOrder order = new StoplossOrder(
                                stock.SecuritySymbol,
                                stock.SecurityName,
                                stock.StoplossPrice,
                                runtime.RemainingVolume,
                                _stoplossOrderExecutedMessageReceiver);

                            runtime.AssociatedStoplossOrder = order;

                            OrderManager.GetInstance().RegisterOrder(order);
                            AppLogger.Default.InfoFormat("Registered order {0}", order);
                        }
                    }
                }
            }
            finally
            {
                _runtimeReadWriteLock.ExitWriteLock();
            }
        }

        private void OnStoplossOrderExecuted(OrderExecutedMessage message)
        {

            if (message == null)
            {
                throw new ArgumentException();
            }

            IOrder order = message.Order;
            int dealVolume = message.DealVolume;
            float dealPrice = message.DealPrice;

            AppLogger.Default.InfoFormat("Order executed. order details: {0}", order);

            if (dealVolume <= 0)
            {
                return;
            }

            _runtimeReadWriteLock.EnterReadLock();
            try
            {
                var stock = _activeExistingStockIndex[order.SecuritySymbol];
                System.Diagnostics.Debug.Assert(stock != null);

                var runtime = _runtimeStockOrders[stock];
                System.Diagnostics.Debug.Assert(runtime != null);
                System.Diagnostics.Debug.Assert(object.ReferenceEquals(runtime.AssociatedStoplossOrder, order));

                runtime.RemainingVolume -= dealVolume;
                System.Diagnostics.Debug.Assert(runtime.RemainingVolume >= 0);
            }
            finally
            {
                _runtimeReadWriteLock.ExitReadLock();
            }
        }

        private void Sell()
        {

        }
        private void OnSellOrderExecuted(OrderExecutedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentException();
            }

            IOrder order = message.Order;
            int dealVolume = message.DealVolume;
            float dealPrice = message.DealPrice;

            AppLogger.Default.InfoFormat("Order executed. order details: {0}", order);
        }

        private void OnNewStockQuoteReady(QuoteResult quote)
        {
            if (!quote.IsValidQuote())
            {
                return;
            }

            Buy(quote.Quote);
        }

        private void OnExistingStockQuoteReady(QuoteResult quote)
        {
            if (!quote.IsValidQuote())
            {
                return;
            }

            if (!_activeExistingStockIndex.ContainsKey(quote.SecuritySymbol))
            {
                CtpSimulator.GetInstance().UnsubscribeQuote(new QuoteSubscription(quote.SecuritySymbol, _existingStockQuotes));

                AppLogger.Default.InfoFormat("Unsubscribe quote {0}", quote.SecuritySymbol);
                return;
            }

            FiveLevelQuote currentQuote = quote.Quote;

            // check if we can issue buy order
            if (_activeNewStockIndex.ContainsKey(currentQuote.SecuritySymbol))
            {
                Buy(currentQuote);
            }

            // check if we can issue sell order
            if (_activeExistingStockIndex.ContainsKey(currentQuote.SecuritySymbol))
            {
                TryPublishSellOrder(currentQuote);
            }
        }

        private void TryPublishSellOrder(FiveLevelQuote quote)
        {
            var stock = _activeExistingStockIndex[quote.SecuritySymbol];

            // for sell order, if current price is up limit, sell it immediately
            float upLimitPrice = (float)ChinaStockHelper.CalculateUpLimit(
                    quote.SecuritySymbol, 
                    quote.SecurityName, 
                    quote.YesterdayClosePrice, 
                    2);

            if (Math.Abs(quote.CurrentPrice - upLimitPrice) < 0.001 // reach up limit
                && Math.Abs(quote.TodayOpenPrice - upLimitPrice) > 0.001 // not 一字板
                && stock.HoldDays > 1)
            {
                // remove stop loss order if have
                // var stoplossOrder = _stoplossOrders.FirstOrDefault(s => s.SecuritySymbol == stock.SecuritySymbol);
                //if (stoplossOrder != default(StoplossOrder))
                {
                    //_stoplossOrders.Remove(stoplossOrder);

                    //OrderManager.GetInstance().UnregisterOrder(stoplossOrder);

                    // need to add back to existing stock index
                    // TODO
                    // TODO
                    // TODO
                    // TODO
                }

                SellOrder order = new SellOrder(
                    stock.SecuritySymbol, 
                    stock.SecurityName, 
                    upLimitPrice, 
                    stock.Volume,
                    _sellOrderExecutedMessageReceiver);

                //_sellOrders.Add(order);

                OrderManager.GetInstance().RegisterOrder(order);

                _activeExistingStockIndex.Remove(stock.SecuritySymbol);

                AppLogger.Default.InfoFormat(
                    "Sell on up limit. Id: {0}, {1}/{2} price {3:0.000}",
                    order.OrderId,
                    order.SecuritySymbol,
                    order.SecurityName,
                    order.SellPrice);
            }
                
            if (!IsValidActionTime(quote.Timestamp.TimeOfDay, _startPublishSellOrderTime, _endPublishSellOrderTime))
            {
                return;
            }

            // check if sell order 
            
        }


    }
}
