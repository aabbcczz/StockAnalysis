using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using log4net;
using log4net.Core;

namespace StockTrading.Utility
{
    sealed class QuotePublisher
    {
        private const int MaxBatchSize = 50;

        private readonly TradingClient _client;
        private readonly int _refreshingIntervalInMillisecond;

        private Timer _timer;
        private bool _isStopped = false;

        private object _publisherLockObj = new object();
        private object _codeListLockObj = new object();

        private List<string> _codeList = new List<string>();

        private CtpSimulator.OnQuoteReadyDelegate _onQuoteReadyCallback = null;

        public QuotePublisher(TradingClient client, int refreshingIntervalInMillisecond)
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

            _timer = new Timer(GetQuote, null, 0, refreshingIntervalInMillisecond);
        }

        public void Stop()
        {
            _timer.Dispose();
            _timer = null;

            lock (_publisherLockObj)
            {
                _isStopped = true;
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

        public void Subscribe(string code)
        {
            lock (_codeListLockObj)
            {
                _codeList.Add(code);

                // remove duplicated quotes
                _codeList = _codeList.GroupBy(s => s).Select(g => g.Key).ToList();
            }

        }
        public void Subscribe(IEnumerable<string> codes)
        {
            lock (_codeListLockObj)
            {
                _codeList.AddRange(codes.ToList());

                // remove duplicated quotes
                _codeList = _codeList.GroupBy(s => s).Select(g => g.Key).ToList();
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

                lock (_codeListLockObj)
                {
                    if (_codeList.Count == 0)
                    {
                        return;
                    }

                    codes = new List<string>(_codeList);
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

                Parallel.ForEach(
                    subsets,
                    subset => 
                        {
                            string[] errors;
                            FiveLevelQuote[] quotes = _client.GetQuote(subset, out errors);

                            PublishQuotes(quotes, errors);
                        });
            }
            catch (Exception ex)
            {
                ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                if (logger != null)
                {
                    logger.ErrorFormat("Exception in getting quote: {0}", ex);
                }
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
