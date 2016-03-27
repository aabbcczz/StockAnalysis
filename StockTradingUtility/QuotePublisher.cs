using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using log4net;
using log4net.Core;
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

        private IDictionary<string, HashSet<QuoteSubscription.QuoteReceiver>> _subscriptions
            = new Dictionary<string, HashSet<QuoteSubscription.QuoteReceiver>>();

        private IDictionary<QuoteSubscription.QuoteReceiver, HashSet<string>> _subscriptionIndex
            = new Dictionary<QuoteSubscription.QuoteReceiver, HashSet<string>>();

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

        public void UnsafeSubscribe(QuoteSubscription subscription)
        {
            if (!_subscriptions.ContainsKey(subscription.SecurityCode))
            {
                _subscriptions.Add(subscription.SecurityCode, new HashSet<QuoteSubscription.QuoteReceiver>());
            }
 
            _subscriptions[subscription.SecurityCode].Add(subscription.Receiver);

            if (!_subscriptionIndex.ContainsKey(subscription.Receiver))
            {
                _subscriptionIndex.Add(subscription.Receiver, new HashSet<string>());
            }

            _subscriptionIndex[subscription.Receiver].Add(subscription.SecurityCode);
        }

        public void UnsafeUnsubscribe(QuoteSubscription subscription)
        {
            if (_subscriptions.ContainsKey(subscription.SecurityCode))
            {
                _subscriptions[subscription.SecurityCode].Remove(subscription.Receiver);
            }

            if (_subscriptionIndex.ContainsKey(subscription.Receiver))
            {
                _subscriptionIndex[subscription.Receiver].Remove(subscription.SecurityCode);
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
                List<string> codes = null;

                _subscriptionReadWriteLock.EnterReadLock();

                try
                {
                    if (_subscriptions.Count == 0)
                    {
                        return;
                    }

                    codes = new List<string>(_subscriptions.Keys);
                }
                finally
                {
                    _subscriptionReadWriteLock.ExitReadLock();
                }

                System.Diagnostics.Debug.Assert(codes.Count > 0);

                List<string[]> subsets = new List<string[]>();
                for (int index = 0; index < codes.Count; index += MaxBatchSize)
                {
                    int count = Math.Min(codes.Count - index, MaxBatchSize);
                    if (count == 0)
                    {
                        break;
                    }

                    var subset = codes.GetRange(index, count).ToArray();

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

        private void PublishQuotes(string[] codes, FiveLevelQuote[] quotes, string[] errors)
        {
            List<QuoteResult> results = null;
            List<QuoteSubscription.QuoteReceiver> receivers = null;
            Dictionary<QuoteSubscription.QuoteReceiver, List<QuoteResult>> subsets = null;

            _subscriptionReadWriteLock.EnterReadLock();

            try
            {
                // find all valid results
                results = Enumerable
                    .Range(0, quotes.Length)
                    .Where(i => _subscriptions.ContainsKey(codes[i]))
                    .Select(i => new QuoteResult(codes[i], quotes[i], errors[i]))
                    .ToList();

                // get all distinct receivers
                receivers = results
                    .SelectMany(r => _subscriptions[r.SecurityCode])
                    .Distinct()
                    .ToList();

                if (results.Count == 0)
                {
                    return;
                }

                subsets = receivers
                    .ToDictionary(
                        r => r, 
                        r => results
                                .Where(q => _subscriptionIndex[r].Contains(q.SecurityCode))
                                .ToList());
            }
            finally
            {
                _subscriptionReadWriteLock.ExitReadLock();
            }

            // move callback out of lock, so that the callback can subscribe/unsubscribe quote
            foreach (var subset in subsets)
            {
                subset.Key(subset.Value);
            }
        }
    }
}
