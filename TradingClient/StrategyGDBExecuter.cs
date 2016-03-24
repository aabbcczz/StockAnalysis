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
        private const string DataFileFolder = "StrategyGDB";
        private readonly TimeSpan _startRunTime = new TimeSpan(9, 29, 0);
        private readonly TimeSpan _endRunTime = new TimeSpan(14, 50, 0);
        private readonly TimeSpan _startPublishStoplossOrderTime = new TimeSpan(9, 30, 0);
        private readonly TimeSpan _endPublishStoplossOrderTime = new TimeSpan(14, 56, 0);
        private readonly TimeSpan _startPublishBuyOrderTime = new TimeSpan(9, 30, 0);
        private readonly TimeSpan _endPublishBuyOrderTime = new TimeSpan(14, 56, 0);
        private readonly TimeSpan _startPublishSellOrderTime = new TimeSpan(14, 56, 0);
        private readonly TimeSpan _endPublishSellOrderTime = new TimeSpan(14, 57, 0);
        
        private List<StrategyGDB.NewStockToBuy> _newStocks = null;
        private List<StrategyGDB.ExistingStockToMaintain> _existingStocks = null;
        
        private HashSet<string> _allCodes = null;
        private Dictionary<string, StrategyGDB.NewStockToBuy> _activeNewStockIndex = null;
        private Dictionary<string, StrategyGDB.ExistingStockToMaintain> _activeExistingStockIndex = null;
        private Dictionary<string, StrategyGDB.ExistingStockToMaintain> _stoplossStockIndex = null;
        private Dictionary<string, StrategyGDB.ExistingStockToMaintain> _sellStockIndex = null;

        private HashSet<StoplossOrder> _stoplossOrders = new HashSet<StoplossOrder>();
        private HashSet<BuyOrder> _buyOrders = new HashSet<BuyOrder>();
        private HashSet<SellOrder> _sellOrders = new HashSet<SellOrder>();

        private object _lockObj = new object();

        public IEnumerable<BuyOrder> ActiveBuyOrders
        {
            get 
            {
                lock (_lockObj)
                {
                    return new List<BuyOrder>(_buyOrders);
                }
            }
        }

        public IEnumerable<StoplossOrder> ActiveStoplossOrders
        {
            get 
            {
                lock (_lockObj)
                {
                    return new List<StoplossOrder>(_stoplossOrders);
                }
            }
        }

        public IEnumerable<SellOrder> ActiveSellOrders
        {
            get 
            {
                lock (_lockObj)
                {
                    return new List<SellOrder>(_sellOrders);
                }
            }
        }

        public StrategyGdbExecuter()
        {
            Initialize();
        }

        private void Initialize()
        {
            StrategyGDB.DataFileReaderWriter rw = new StrategyGDB.DataFileReaderWriter(DataFileFolder);

            rw.Read();

            _newStocks = rw.NewStocks.ToList();
            _existingStocks = rw.ExistingStocks.ToList();

            var allCodes = _newStocks
                .Select(n => n.SecurityCode)
                .Union(_existingStocks.Select(e => e.SecurityCode));
                

            _allCodes = new HashSet<string>(allCodes);

            if (AppLogger.Default.IsDebugEnabled)
            {
                AppLogger.Default.DebugFormat(
                    "GDB strategy executer: loaded codes {0}",
                    string.Join(",", _allCodes));
            }

            _activeNewStockIndex = _newStocks.ToDictionary(s => s.SecurityCode, s => s);
            _activeExistingStockIndex = _existingStocks.ToDictionary(s => s.SecurityCode, s => s);

            // all existing stocks should have stop loss order
            _stoplossStockIndex = _existingStocks.ToDictionary(s => s.SecurityCode, s => s);

            // all existing stocks might be sold
            _sellStockIndex = _existingStocks.ToDictionary(s => s.SecurityCode, s => s);
        }

        public void Run()
        {
            if (_allCodes.Count == 0)
            {
                return;
            }

            if (!WaitForValidTime(_startRunTime, _endRunTime))
            {
                AppLogger.Default.ErrorFormat("Wait for valid trading time failed");
                return;
            }

            CtpSimulator.GetInstance().RegisterQuoteReadyCallback(OnQuoteReadyCallback);
            CtpSimulator.GetInstance().SubscribeQuote(_allCodes);

            StoplossOrderManager.GetInstance().OnStoplossOrderExecuted += OnStoplossOrderExecutedCallback;
            BuyOrderManager.GetInstance().OnBuyOrderExecuted += OnBuyOrderExecutedCallback;
            SellOrderManager.GetInstance().OnSellOrderExecuted += OnSellOrderExecutedCallback;

            PublishStopLossOrders();
        }

        private bool WaitForValidTime(TimeSpan startTime, TimeSpan endTime)
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

        private void PublishStopLossOrders()
        {
            if (_stoplossStockIndex.Count == 0)
            {
                return;
            }

            if (!WaitForValidTime(_startPublishStoplossOrderTime, _endPublishStoplossOrderTime))
            {
                AppLogger.Default.ErrorFormat("Wait for valid trading time to publish stop loss order failed");
                return;
            }

            lock (_lockObj)
            {
                foreach (var kvp in _stoplossStockIndex)
                {
                    StoplossOrder order = new StoplossOrder(
                        kvp.Value.SecurityCode,
                        kvp.Value.SecurityName,
                        kvp.Value.StoplossPrice,
                        kvp.Value.Volume);

                    _stoplossOrders.Add(order);
                }

                foreach (var order in _stoplossOrders)
                {
                    // remove sell order if have
                    var sellOrder = _sellOrders.FirstOrDefault(s => s.SecurityCode == order.SecurityCode);
                    if (sellOrder != default(SellOrder))
                    {
                        _sellOrders.Remove(sellOrder);
                        SellOrderManager.GetInstance().UnregisterSellOrder(sellOrder);

                    }

                    StoplossOrderManager.GetInstance().RegisterStoplossOrder(order);
                }
            }
        }

        private void OnStoplossOrderExecutedCallback(StoplossOrder order, float dealPrice, int dealVolume)
        {
            lock (_lockObj)
            {
                if (!_stoplossOrders.Contains(order))
                {
                    return;
                }

                // remove from existing stock index, so that no any sell order can be created for this stock
                _activeExistingStockIndex.Remove(order.SecurityCode);
            }

            AppLogger.Default.InfoFormat(
                "Stop loss order executed. Id: {0}, {1}/{2}, price {3:0.000}, volume {4}, remaining volume {5}",
                order.OrderId,
                order.SecurityCode,
                order.SecurityName,
                dealPrice,
                dealVolume,
                order.RemainingVolume);
        }

        private void OnBuyOrderExecutedCallback(BuyOrder order, float dealPrice, int dealVolume)
        {
            lock (_lockObj)
            {
                if (!_buyOrders.Contains(order))
                {
                    return;
                }
            }

            AppLogger.Default.InfoFormat(
                "Buy order executed. Id: {0}, {1}/{2}, price {3:0.000}, volume {4}, remaining volume {5}, remaining capital {6:0.000}",
                order.OrderId,
                order.SecurityCode,
                order.SecurityName,
                dealPrice,
                dealVolume,
                order.RemainingVolumeCanBeBought,
                order.RemainingCapitalCanBeUsed);
            
        }

        private void OnSellOrderExecutedCallback(SellOrder order, float dealPrice, int dealVolume)
        {
            lock (_lockObj)
            {
                if (!_sellOrders.Contains(order))
                {
                    return;
                }

                // remove from existing stock index, so that no any stop loss order can be created for this stock
                _activeExistingStockIndex.Remove(order.SecurityCode);
            }

            AppLogger.Default.InfoFormat(
                "Sell order executed. Id: {0}, {1}/{2}, price {3:0.000}, volume {4}, remaining volume {5}",
                order.OrderId,
                order.SecurityCode,
                order.SecurityName,
                dealPrice,
                dealVolume,
                order.RemainingVolume);
            
        }

        private void OnQuoteReadyCallback(FiveLevelQuote[] quotes, string[] errors)
        {
            for (int i = 0; i < quotes.Length; ++i)
            {
                if (!string.IsNullOrEmpty(errors[i]))
                {
                    continue;
                }

                if (!_allCodes.Contains(quotes[i].SecurityCode))
                {
                    continue;
                }

                FiveLevelQuote currentQuote = quotes[i];

                lock (_lockObj)
                {
                    // check if we can issue buy order
                    if (_activeNewStockIndex.ContainsKey(currentQuote.SecurityCode))
                    {
                        TryPublishBuyOrder(currentQuote);
                    }

                    // check if we can issue sell order
                    if (_activeExistingStockIndex.ContainsKey(currentQuote.SecurityCode))
                    {
                        TryPublishSellOrder(currentQuote);
                    }
                }
            }
        }

        private bool IsValidTime(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
        {
            return (currentTime >= startTime && currentTime <= endTime);
        }

        private void TryPublishSellOrder(FiveLevelQuote quote)
        {
            var stock = _activeExistingStockIndex[quote.SecurityCode];

            // for sell order, if current price is up limit, sell it immediately
            float upLimitPrice = (float)ChinaStockHelper.CalculateUpLimit(
                    quote.SecurityCode, 
                    quote.SecurityName, 
                    quote.YesterdayClosePrice, 
                    2);

            if (Math.Abs(quote.CurrentPrice - upLimitPrice) < 0.001 // reach up limit
                && Math.Abs(quote.TodayOpenPrice - upLimitPrice) > 0.001 // not 一字板
                && stock.HoldDays > 1)
            {
                // remove stop loss order if have
                var stoplossOrder = _stoplossOrders.FirstOrDefault(s => s.SecurityCode == stock.SecurityCode);
                if (stoplossOrder != default(StoplossOrder))
                {
                    _stoplossOrders.Remove(stoplossOrder);

                    StoplossOrderManager.GetInstance().UnregisterStoplossOrder(stoplossOrder);

                    // need to add back to existing stock index
                    // TODO
                    // TODO
                    // TODO
                    // TODO
                }

                SellOrder order = new SellOrder(stock.SecurityCode, stock.SecurityName, upLimitPrice, stock.Volume);

                _sellOrders.Add(order);

                SellOrderManager.GetInstance().RegisterSellOrder(order);

                _activeExistingStockIndex.Remove(stock.SecurityCode);

                AppLogger.Default.InfoFormat(
                    "Sell on up limit. Id: {0}, {1}/{2} price {3:0.000}",
                    order.OrderId,
                    order.SecurityCode,
                    order.SecurityName,
                    order.SellPrice);
            }
                
            if (!IsValidTime(quote.Timestamp.TimeOfDay, _startPublishSellOrderTime, _endPublishSellOrderTime))
            {
                return;
            }

            // check if sell order 
            
        }

        private void TryPublishBuyOrder(FiveLevelQuote quote)
        {
            if (!IsValidTime(quote.Timestamp.TimeOfDay, _startPublishBuyOrderTime, _endPublishBuyOrderTime))
            {
                return;
            }

            var stock = _activeNewStockIndex[quote.SecurityCode];
            
            if (stock.DateToBuy.Date != DateTime.Today)
            {
                // remove from active new stock
                _activeNewStockIndex.Remove(quote.SecurityCode);

                AppLogger.Default.WarnFormat(
                    "The buy date of stock {0:yyyy-MM-dd} is not today. {1}/{2}",
                    stock.DateToBuy,
                    stock.SecurityCode,
                    stock.SecurityName);

                return;
            }

            // judge if open price is in valid range
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
                    _activeNewStockIndex.Remove(quote.SecurityCode);

                    AppLogger.Default.InfoFormat(
                        "Failed to buy stock because open price is out of range. {0}/{1} open {2:0.000} out of [{3:0.000}, {4:0.000}]",
                        stock.SecurityCode,
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
                }
            }

            // only buy those stock which has been raised over open price
            if (quote.CurrentPrice > quote.TodayOpenPrice)
            {
                float downLimitPrice = (float)ChinaStockHelper.CalculateDownLimit(stock.SecurityCode, stock.SecurityName, quote.YesterdayClosePrice, 2);
                float minBuyPrice = Math.Max(stock.StoplossPrice, downLimitPrice);

                BuyInstruction instruction = new BuyInstruction(
                    stock.SecurityCode, 
                    stock.SecurityName,
                    minBuyPrice,
                    stock.ActualMaxBuyPrice,
                    stock.ActualMaxBuyPrice,
                    stock.TotalCapital,
                    (int)(stock.TotalCapital / stock.ActualMaxBuyPrice));

                BuyOrder order = new BuyOrder(instruction);

                _buyOrders.Add(order);

                BuyOrderManager.GetInstance().RegisterBuyOrder(order);

                // remove active new stock after issued buy order.
                _activeNewStockIndex.Remove(stock.SecurityCode);
            }
        }
    }
}
