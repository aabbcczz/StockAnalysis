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
        private object _codesLockObj = new object();

        private IDictionary<string, int> _subscribedCodes = new Dictionary<string, int>();

        private CtpSimulator.OnQuoteReadyDelegate _onQuoteReadyCallback = null;

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

        public void RegisterQuoteReadyCallback(CtpSimulator.OnQuoteReadyDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }

            lock (_publisherLockObj)
            {
                _onQuoteReadyCallback += callback;
            }
        }

        public void UnsafeSubscribe(string code)
        {
            if (!_subscribedCodes.ContainsKey(code))
            {
                _subscribedCodes.Add(code, 1);
            }
            else
            {
                _subscribedCodes[code]++;
            }
        }

        public void UnsafeUnsubscribe(string code)
        {

            if (_subscribedCodes.ContainsKey(code))
            {
                int refCount = _subscribedCodes[code];

                --refCount;

                if (refCount > 0)
                {
                    _subscribedCodes[code] = refCount;
                }
                else
                {
                    _subscribedCodes.Remove(code);
                }
            }
        }

        public void Subscribe(string code)
        {
            lock (_codesLockObj)
            {
                UnsafeSubscribe(code);
            }
        }

        public void Unsubscribe(string code)
        {
            lock (_codesLockObj)
            {
                UnsafeUnsubscribe(code);
            }
        }

        public void Subscribe(IEnumerable<string> codes)
        {
            lock (_codesLockObj)
            {
                foreach (var code in codes)
                {
                    UnsafeSubscribe(code);
                }
            }
        }

        public void Unsubscribe(IEnumerable<string> codes)
        {
            lock (_codesLockObj)
            {
                foreach (var code in codes)
                {
                    UnsafeUnsubscribe(code);
                }
            }
        }

        private void GetQuote(object state)
        {
            if (!Monitor.TryEnter(_publisherLockObj))
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
                // get a duplicate quote list to avoid lock quote list too long
                List<string> codes = null;

                lock (_codesLockObj)
                {
                    if (_subscribedCodes.Count == 0)
                    {
                        return;
                    }

                    codes = new List<string>(_subscribedCodes.Keys);
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
                            
                            PublishQuotes(quotes, errors);
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

        private void PublishQuotes(FiveLevelQuote[] quotes, string[] errors)
        {
            if (_onQuoteReadyCallback != null)
            {
                _onQuoteReadyCallback(quotes, errors);
            }
        }
    }
}
