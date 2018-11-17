using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using StockAnalysis.Common.Utility;

using StockAnalysis.Share;

namespace StockTrading.Utility
{
    sealed class QuotePublisher
    {
        private const int MaxBatchSize = 50;

        private TradingClient _client;
        private readonly int _refreshingIntervalInMillisecond;
        private readonly bool _enableSinaQuote;

        private Timer _timer;
        private bool _isStopped = false;

        private object _publisherLockObj = new object();
        private ReaderWriterLockSlim _subscriptionReadWriteLock = new ReaderWriterLockSlim();

        private IDictionary<string, HashSet<WaitableConcurrentQueue<QuoteResult>>> _subscriptions
            = new Dictionary<string, HashSet<WaitableConcurrentQueue<QuoteResult>>>();

        private IDictionary<WaitableConcurrentQueue<QuoteResult>, HashSet<string>> _subscriptionIndex
            = new Dictionary<WaitableConcurrentQueue<QuoteResult>, HashSet<string>>();

        public QuotePublisher(TradingClient client, int refreshingIntervalInMillisecond, bool enableSinaQuote)
        {
            if (client == null)
            {
                throw new ArgumentNullException();
            }

            if (refreshingIntervalInMillisecond <= 0)
            {
                throw new ArgumentOutOfRangeException("Refreshing interval must be greater than 0");
            }

            _client = client;
            _refreshingIntervalInMillisecond = refreshingIntervalInMillisecond;
            _enableSinaQuote = enableSinaQuote;

            _timer = new Timer(GetQuote, null, 0, refreshingIntervalInMillisecond);
        }

        public void Stop()
        {
            _timer.Dispose();
            _timer = null;

            lock (_publisherLockObj)
            {
                _isStopped = true;

                _client = null;
            }
        }

        private void UnsafeSubscribe(QuoteSubscription subscription)
        {
            if (!_subscriptions.ContainsKey(subscription.SecuritySymbol))
            {
                _subscriptions.Add(subscription.SecuritySymbol, new HashSet<WaitableConcurrentQueue<QuoteResult>>());
            }
 
            _subscriptions[subscription.SecuritySymbol].Add(subscription.ResultQueue);

            if (!_subscriptionIndex.ContainsKey(subscription.ResultQueue))
            {
                _subscriptionIndex.Add(subscription.ResultQueue, new HashSet<string>());
            }

            _subscriptionIndex[subscription.ResultQueue].Add(subscription.SecuritySymbol);
        }

        private void UnsafeUnsubscribe(QuoteSubscription subscription)
        {
            if (_subscriptions.ContainsKey(subscription.SecuritySymbol))
            {
                _subscriptions[subscription.SecuritySymbol].Remove(subscription.ResultQueue);
            }

            if (_subscriptionIndex.ContainsKey(subscription.ResultQueue))
            {
                _subscriptionIndex[subscription.ResultQueue].Remove(subscription.SecuritySymbol);
            }
        }

        public void Subscribe(QuoteSubscription subscription)
        {
            _subscriptionReadWriteLock.EnterWriteLock();

            try
            {
                UnsafeSubscribe(subscription);
            }
            finally
            {
                _subscriptionReadWriteLock.ExitWriteLock();
            }
        }

        public void Unsubscribe(QuoteSubscription subscription)
        {
            _subscriptionReadWriteLock.EnterWriteLock();

            try
            {
                UnsafeUnsubscribe(subscription);
            }
            finally
            {
                _subscriptionReadWriteLock.ExitWriteLock();
            }
        }

        public void Subscribe(IEnumerable<QuoteSubscription> subscriptions)
        {
            _subscriptionReadWriteLock.EnterWriteLock();

            try
            {
                foreach (var subscription in subscriptions)
                {
                    UnsafeSubscribe(subscription);
                }
            }
            finally
            {
                _subscriptionReadWriteLock.ExitWriteLock();
            }
        }

        public void Unsubscribe(IEnumerable<QuoteSubscription> subscriptions)
        {
            _subscriptionReadWriteLock.EnterWriteLock();

            try
            {
                foreach (var subscription in subscriptions)
                {
                    UnsafeUnsubscribe(subscription);
                }
            }
            finally
            {
                _subscriptionReadWriteLock.ExitWriteLock();
            }
        }

        private void GetQuote(object state)
        {
            if (!Monitor.TryEnter(_publisherLockObj))
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

                // get a duplicate quote list to avoid lock quote list too long
                List<string> symbols = null;

                _subscriptionReadWriteLock.EnterReadLock();

                try
                {
                    if (_subscriptions.Count == 0)
                    {
                        return;
                    }

                    symbols = new List<string>(_subscriptions.Keys);
                }
                finally
                {
                    _subscriptionReadWriteLock.ExitReadLock();
                }

                System.Diagnostics.Debug.Assert(symbols.Count > 0);

                List<string[]> subsets = new List<string[]>();
                for (int index = 0; index < symbols.Count; index += MaxBatchSize)
                {
                    int count = Math.Min(symbols.Count - index, MaxBatchSize);
                    if (count == 0)
                    {
                        break;
                    }

                    var subset = symbols.GetRange(index, count).ToArray();

                    subsets.Add(subset);
                }

                System.Diagnostics.Debug.Assert(subsets.Count > 0);

                Parallel.ForEach(
                    subsets,
                    async subset => 
                        {
                            List<SinaStockQuote> sinaQuotes = null;
                            if (_enableSinaQuote)
                            {
                                sinaQuotes = await SinaStockQuoteInterface.GetQuote(subset);
                            }

                            string[] errors;
                            FiveLevelQuote[] quotes = _client.GetQuote(subset, out errors);

                            if (_enableSinaQuote)
                            {
                                if (quotes.Length != sinaQuotes.Count)
                                {
                                    throw new InvalidOperationException("The count of sina quote does not match tdx quote");
                                }

                                for (int i = 0; i < quotes.Length; ++i)
                                {
                                    if (quotes[i] != null)
                                    {
                                        quotes[i].DealAmount = sinaQuotes[i].DealAmount;
                                        quotes[i].DealVolumeInHand = sinaQuotes[i].DealVolumeInHand;
                                    }
                                }
                            }

                            if (AppLogger.Default.IsDebugEnabled)
                            {
                                for (int i = 0; i < quotes.Length; ++i)
                                {
                                    if (!string.IsNullOrEmpty(errors[i]))
                                    {
                                        AppLogger.Default.DebugFormat("Fail to get quote for {0}: {1}", subset[i], errors[i]);
                                    }
                                }
                            }
                            
                            PublishQuotes(subset, quotes, errors);
                        });
            }
            catch (Exception ex)
            {
                AppLogger.Default.ErrorFormat("Exception in getting quote: {0}", ex);
            }
            finally
            {
                Monitor.Exit(_publisherLockObj);
            }
        }

        private void PublishQuotes(string[] symbols, FiveLevelQuote[] quotes, string[] errors)
        {
            List<QuoteResult> results = null;
            List<WaitableConcurrentQueue<QuoteResult>> resultQueues = null;
            Dictionary<WaitableConcurrentQueue<QuoteResult>, List<QuoteResult>> subsets = null;

            _subscriptionReadWriteLock.EnterReadLock();

            try
            {
                // find all valid results
                results = Enumerable
                    .Range(0, quotes.Length)
                    .Where(i => _subscriptions.ContainsKey(symbols[i]))
                    .Select(i => new QuoteResult(symbols[i], quotes[i], errors[i]))
                    .ToList();

                // get all distinct resultQueues
                resultQueues = results
                    .SelectMany(r => _subscriptions[r.SecuritySymbol])
                    .Distinct()
                    .ToList();

                if (results.Count == 0)
                {
                    return;
                }

                subsets = resultQueues
                    .ToDictionary(
                        r => r, 
                        r => results
                                .Where(q => _subscriptionIndex[r].Contains(q.SecuritySymbol))
                                .ToList());
            }
            finally
            {
                _subscriptionReadWriteLock.ExitReadLock();
            }

            // put quotes to queues 
            foreach (var subset in subsets)
            {
                subset.Key.Add(subset.Value);
            }
        }
    }
}
