using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace StockTrading.Utility
{
    public sealed class CtpSimulator
    {
        private static CtpSimulator _instance = null;

        private object _syncObj = new object();

        private TradingClient _client = null;
        private bool _initialized = false;

        private Thread _quoteThread = null;

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
                lock (_syncObj)
                {
                    if (!_initialized)
                    {
                        TradingEnvironment.Initialize();

                        _client = new TradingClient();

                        _quoteThread = new Thread(QuoteThreadStart);
                        _quoteThread.Start();

                        _initialized = true;
                    }
                }
            }

            _client.LogOff();

            return _client.LogOn(address, port, protocolVersion, yybId, accountNo, accountType, tradeAccount, tradePassword, communicationPassword, out error);
        }

        public void UnInitialize()
        {
            if (_initialized)
            {
                lock (_syncObj)
                {
                    if (_initialized)
                    {
                        _quoteThread.Join();
                        _quoteThread = null;

                        _client.LogOff();
                        _client = null;

                        TradingEnvironment.UnInitialize();

                        _initialized = false;
                    }
                }
            }
        }

        private void QuoteThreadStart()
        {

        }
    }
}
