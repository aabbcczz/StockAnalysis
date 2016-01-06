using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RealTrading
{
    sealed class TradingClient : IDisposable
    {
        public const int InvalidClientId = -1;
        private const int MaxErrorStringSize = 1024;
        private const int MaxResultStringSize = 1024 * 1024;

        private bool _disposed = false;

        private Dictionary<Exchange, string> _shareholderCodes = new Dictionary<Exchange, string>();

        public int ClientId { get; private set; }

        public TradingClient()
        {
            ClientId = InvalidClientId;
        }

        ~TradingClient()
        {
            Dispose();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        private void CheckLoggedOn()
        {
            if (!IsLoggedOn())
            {
                throw new InvalidOperationException("client has not logged on yet");
            }
        }

        public bool LogOn(
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
            CheckDisposed();

            error = string.Empty;

            if (IsLoggedOn())
            {
                throw new InvalidOperationException("Client has logged on");
            }

            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            int clientId = TdxWrapper.Logon(
                address,
                port,
                protocolVersion,
                yybId,
                accountNo,
                tradeAccount,
                tradePassword,
                communicationPassword,
                errorInfo);

            if (clientId < 0)
            {
                error = errorInfo.ToString();
            }
            else
            {
                ClientId = clientId;

                if (!InitializeAfterLoggedOn(out error))
                {
                    LogOff();

                    error = "Failed to initialize client after logged on. " + error;
                }
            }

            return IsLoggedOn();
        }

        private bool InitializeAfterLoggedOn(out string error)
        {
            TabulateData result;

            if (!QueryData(DataCategory.ShareholderRegistryCode, out result, out error))
            {
                return false;
            }

            var registries = QueryShareholderRegistryResult.ExtractFrom(result);

            foreach (var registry in registries)
            {
                _shareholderCodes.Add(registry.Exchange, registry.ShareholderCode);
            }

            return true;
        }

        public bool IsLoggedOn()
        {
            CheckDisposed();

            return ClientId != InvalidClientId;
        }
    
        public void LogOff()
        {
            CheckDisposed();

            if (!IsLoggedOn())
            {
                return;
            }

            TdxWrapper.Logoff(ClientId);

            ClientId = InvalidClientId;
        }

        public bool QueryData(DataCategory category, out TabulateData result, out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.QueryData(ClientId, (int)category, resultInfo, errorInfo);

            error = errorInfo.ToString();
            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return  succeeded;
        }

        private IntPtr[] AllocateStringBuffers(int count, int bufferSize)
        {
            IntPtr[] ptrs = new IntPtr[count];

            for (int i = 0; i < count; ++i)
            {
                ptrs[i] = Marshal.AllocHGlobal(bufferSize);
            }

            return ptrs;
        }

        private string[] ConvertStringBufferToString(IntPtr[] ptrs)
        {
            string[] strings = new string[ptrs.Length];

            for (int i = 0; i < strings.Length; ++i)
            {
                strings[i] = Marshal.PtrToStringAnsi(ptrs[i]);
            }

            return strings;
        }

        private void FreeStringBuffers(IntPtr[] ptrs)
        {
            foreach (var ptr in ptrs)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public bool[] QueryData(DataCategory[] categories, out TabulateData[] results, out string[] errors)
        {
            CheckDisposed();
            CheckLoggedOn();

            if (categories == null || categories.Length == 0)
            {
                throw new ArgumentNullException();
            }

            IntPtr[] resultInfos = AllocateStringBuffers(categories.Length, MaxResultStringSize);
            IntPtr[] errorInfos = AllocateStringBuffers(categories.Length, MaxErrorStringSize);

            try
            {
                int[] categoryArray = new int[categories.Length];
                for (int i = 0; i < categoryArray.Length; ++i)
                {
                    categoryArray[i] = (int)categories[i];
                }

                TdxWrapper.QueryDatas(ClientId, categoryArray, categoryArray.Length, resultInfos, errorInfos);

                string[] resultStrings = ConvertStringBufferToString(resultInfos);
                errors = ConvertStringBufferToString(errorInfos);

                bool[] succeeds = new bool[categories.Length];
                results = new TabulateData[categories.Length];

                for (int i = 0; i < results.Length; ++i)
                {
                    succeeds[i] = string.IsNullOrEmpty(errors[i]);

                    results[i] = succeeds[i] ? TabulateData.Parse(resultStrings[i]) : null;
                }

                return succeeds;
            }
            finally
            {
                FreeStringBuffers(resultInfos);
                FreeStringBuffers(errorInfos);
            }
        }

        public bool GetQuote(string securityCode, out TabulateData result, out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.GetQuote(ClientId, securityCode, resultInfo, errorInfo);

            error = errorInfo.ToString();
            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return succeeded;
        }

        public bool[] GetQuote(string[] securityCodes, out TabulateData[] results, out string[] errors)
        {
            CheckDisposed();
            CheckLoggedOn();

            if (securityCodes == null || securityCodes.Length == 0)
            {
                throw new ArgumentNullException();
            }

            IntPtr[] resultInfos = AllocateStringBuffers(securityCodes.Length, MaxResultStringSize);
            IntPtr[] errorInfos = AllocateStringBuffers(securityCodes.Length, MaxErrorStringSize);

            try
            {
                TdxWrapper.GetQuotes(ClientId, securityCodes, securityCodes.Length, resultInfos, errorInfos);

                string[] resultStrings = ConvertStringBufferToString(resultInfos);
                errors = ConvertStringBufferToString(errorInfos);

                bool[] succeeds = new bool[securityCodes.Length];
                results = new TabulateData[securityCodes.Length];

                for (int i = 0; i < results.Length; ++i)
                {
                    succeeds[i] = string.IsNullOrEmpty(errors[i]);

                    results[i] = succeeds[i] ? TabulateData.Parse(resultStrings[i]) : null;
                }

                return succeeds;
            }
            finally
            {
                FreeStringBuffers(resultInfos);
                FreeStringBuffers(errorInfos);
            }
        }

        public string GetShareholderCode(Exchange exchange)
        {
            CheckDisposed();
            CheckLoggedOn();

            if (_shareholderCodes.ContainsKey(exchange))
            {
                return _shareholderCodes[exchange];
            }
            else
            {
                return string.Empty;
            }
        }

        public bool SendOrder(
            OrderCategory orderCategory, 
            OrderPriceType orderPriceType, 
            string securityCode, 
            float price, 
            int quantity, 
            out TabulateData result, 
            out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            result = null;
            error = string.Empty;

            Exchange exchange = Exchange.GetTradeableExchangeForSecurity(securityCode);
            if (exchange == null)
            {
                error = string.Format("can't find exchange for trading security '{0}'", securityCode);
                return false;
            }

            string shareholderCode = GetShareholderCode(exchange);

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.SendOrder(
                ClientId,
                (int)orderCategory,
                (int)orderPriceType,
                shareholderCode,
                securityCode, 
                price,
                quantity,
                resultInfo, 
                errorInfo);

            error = errorInfo.ToString();

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return succeeded;
        }

        public bool CancelOrder(string code, int orderId, out TabulateData result, out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            Exchange exchange = Exchange.GetTradeableExchangeForSecurity(code);
            if (exchange == null)
            {
                result = null;
                error = "Invalid code";
                return false;
            }

            TdxWrapper.CancelOrder(ClientId, exchange.Id.ToString(), orderId.ToString(), resultInfo, errorInfo);

            error = errorInfo.ToString();
            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return succeeded;
        }

        public bool Payback(float amount, out TabulateData result, out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.Repay(ClientId, amount.ToString("0.00"), resultInfo, errorInfo);

            error = errorInfo.ToString();
            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return succeeded;
        }

        public bool QueryHistoryData(
            HistoryDataCategory category, 
            DateTime startDate, 
            DateTime endDate, 
            out TabulateData result, 
            out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.QueryHistoryData(
                ClientId, 
                (int)category, 
                startDate.ToString("yyyyMMdd"), 
                endDate.ToString("yyyyMMdd"),
                resultInfo, 
                errorInfo);

            error = errorInfo.ToString();
            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return succeeded;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (IsLoggedOn())
                {
                    LogOff();
                }

                _disposed = true;
            }
        }
    }
}
