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
    sealed class QuoteController
    {
        public delegate void OnQuoteReadyDelegate(FiveLevelQuote[] quotes, string[] errors);

        private readonly TradingClient _client;
        private readonly int _refreshingIntervalInMillisecond;

        private Timer _timer;
        private object _quoteLockObj = new object();
        private object _codeListLockObj = new object();

        private List<string> _codeList = new List<string>();

        private OnQuoteReadyDelegate _onQuoteReadyCallback = null;

        public QuoteController(TradingClient client, int refreshingIntervalInMillisecond)
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
        }

        public void RegisterQuoteReadyCallback(OnQuoteReadyDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }

            lock (_quoteLockObj)
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
            if (!Monitor.TryEnter(_quoteLockObj))
            {
                // ignore this refresh because previous refreshing is still on going.
                return;
            }

            try
            {
                // get a duplicate quote list to avoid lock quote list too long
                string[] codes = null;

                lock (_codeListLockObj)
                {
                    if (_codeList.Count == 0)
                    {
                        return;
                    }

                    codes = _codeList.ToArray();
                }

                System.Diagnostics.Debug.Assert(codes.Length > 0);

                string[] errors;
                FiveLevelQuote[] quotes = _client.GetQuote(codes, out errors);

                PublishQuotes(quotes, errors);
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
                Monitor.Exit(_quoteLockObj);
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
