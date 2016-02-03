using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace StockTrading.Utility
{
    public sealed class TradingClient : IDisposable
    {
        public const int InvalidClientId = -1;
        private const int MaxErrorStringSize = 1024;
        private const int MaxResultStringSize = 128 * 1024;

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
            var registries = QueryShareholderRegistry(out error);

            if (registries == null)
            {
                return false;
            }

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

        public QueryCapitalResult QueryCapital(out string error)
        {
            TabulateData data;
            if (!QueryData(DataCategory.Capital, out data, out error))
            {
                return null;
            }

            var results = QueryCapitalResult.ExtractFrom(data);

            if (results == null || results.Count() == 0)
            {
                error = "QueryCapital succeeded, but no result";
                return null;
            }

            return results.First();
        }

        public IEnumerable<QueryStockResult> QueryStock(out string error)
        {
            TabulateData data;
            if (!QueryData(DataCategory.Stock, out data, out error))
            {
                return new List<QueryStockResult>();
            }

            return QueryStockResult.ExtractFrom(data);
        }

        public IEnumerable<QueryShareholderRegistryResult> QueryShareholderRegistry(out string error)
        {
            TabulateData data;
            if (!QueryData(DataCategory.ShareholderRegistryCode, out data, out error))
            {
                return new List<QueryShareholderRegistryResult>();
            }

            return QueryShareholderRegistryResult.ExtractFrom(data);
        }

        public IEnumerable<QueryGeneralOrderResult> QuerySubmittedOrderToday(out string error)
        {
            TabulateData data;
            if (!QueryData(DataCategory.OrderSubmittedToday, out data, out error))
            {
                return new List<QueryGeneralOrderResult>();
            }

            return QueryGeneralOrderResult.ExtractFrom(data);
        }

        public IEnumerable<QuerySucceededOrderResult> QuerySucceededOrderToday(out string error)
        {
            TabulateData data;
            if (!QueryData(DataCategory.OrderSucceededToday, out data, out error))
            {
                return new List<QuerySucceededOrderResult>();
            }

            return QuerySucceededOrderResult.ExtractFrom(data);
        }

        public IEnumerable<QueryGeneralOrderResult> QueryCancellableOrder(out string error)
        {
            TabulateData data;
            if (!QueryData(DataCategory.CancellableOrder, out data, out error))
            {
                return new List<QueryGeneralOrderResult>();
            }

            return QueryGeneralOrderResult.ExtractFrom(data);
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
                int[] categoryArray = categories.Select(c => (int)c).ToArray();
 
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

        public FiveLevelQuote GetQuote(string securityCode, out string error)
        {
            TabulateData data;
            if (!GetQuote(securityCode, out data, out error))
            {
                return null;
            }

            var results = FiveLevelQuote.ExtractFrom(data);

            if (results == null || results.Count() == 0)
            {
                error = "GetQuote succeeded, but not result";
                return null;
            }

            return results.First();
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

        public FiveLevelQuote[] GetQuote(string[] securityCodes, out string[] errors)
        {
            TabulateData[] data;
            bool[] succeeds = GetQuote(securityCodes, out data, out errors);


            FiveLevelQuote[] quotes = new FiveLevelQuote[securityCodes.Length];

            for (int i = 0; i < securityCodes.Length; ++i)
            {
                if (!succeeds[i])
                {
                    quotes[i] = null;
                }
                else
                {
                    var results = FiveLevelQuote.ExtractFrom(data[i]);

                    if (results == null || results.Count() == 0)
                    {
                        errors[i] = "GetQuote succeeded, but not result";
                        quotes[i] = null;
                    }
                    else
                    {
                        quotes[i] = results.First();
                    }
                }
            }

            return quotes;
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

        public string GetShareholderCode(string securityCode)
        {
            Exchange exchange = Exchange.GetTradeableExchangeForSecurity(securityCode);
            if (exchange == null)
            {
                return string.Empty;
            }

            string shareholderCode = GetShareholderCode(exchange);
            return shareholderCode;
        }


        public SendOrderResult SendOrder(OrderRequest request, out string error)
        {
            TabulateData data;

            if (!SendOrder(request, out data, out error))
            {
                return null;
            }

            var results = SendOrderResult.ExtractFrom(data);
            if (results == null || results.Count() == 0)
            {
                error = "SendOrder succeeded, but no result";
                return null;
            }

            return results.First();
        }

        public bool SendOrder(OrderRequest request, out TabulateData result, out string error)
        {
            CheckDisposed();
            CheckLoggedOn();

            result = null;
            error = string.Empty;

            string shareholderCode = GetShareholderCode(request.SecurityCode);

            StringBuilder resultInfo = new StringBuilder(MaxResultStringSize);
            StringBuilder errorInfo = new StringBuilder(MaxErrorStringSize);

            TdxWrapper.SendOrder(
                ClientId,
                (int)request.Category,
                (int)request.PricingType,
                shareholderCode,
                request.SecurityCode, 
                request.Price,
                request.Volume,
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

        public SendOrderResult[] SendOrder(OrderRequest[] requests, out string[] errors)
        {
            TabulateData[] data;

            bool[] succeeds = SendOrder(requests, out data, out errors);

            SendOrderResult[] sendOrderResults = new SendOrderResult[requests.Length];

            for (int i = 0; i < sendOrderResults.Length; ++i)
            {
                if (!succeeds[i])
                {
                    sendOrderResults[i] = null;
                }
                else
                {
                    var results = SendOrderResult.ExtractFrom(data[i]);
                    if (results == null || results.Count() == 0)
                    {
                        errors[i] = "SendOrder succeeded, but no result";
                        sendOrderResults[i] = null;
                    }
                    else
                    {
                        sendOrderResults[i] = results.First();
                    }
                }
            }

            return sendOrderResults;
        }

        public bool[] SendOrder(OrderRequest[] requests, out TabulateData[] results, out string[] errors)
        {
            CheckDisposed();
            CheckLoggedOn();

            if (requests == null || requests.Length == 0)
            {
                throw new ArgumentNullException();
            }

            IntPtr[] resultInfos = AllocateStringBuffers(requests.Length, MaxResultStringSize);
            IntPtr[] errorInfos = AllocateStringBuffers(requests.Length, MaxErrorStringSize);

            try
            {
                var shareholderCodes = requests.Select(req => GetShareholderCode(req.SecurityCode)).ToArray();
                var categories = requests.Select(req => (int)req.Category).ToArray();
                var priceTypes = requests.Select(req => (int)req.PricingType).ToArray();
                var securityCodes = requests.Select(req => req.SecurityCode).ToArray();
                var prices = requests.Select(req => req.Price).ToArray();
                var quantities = requests.Select(req => req.Volume).ToArray();

                TdxWrapper.SendOrders(
                    ClientId,
                    categories,
                    priceTypes,
                    shareholderCodes,
                    securityCodes,
                    prices,
                    quantities,
                    requests.Count(),
                    resultInfos,
                    errorInfos);
                    
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

        public bool CancelOrder(string code, int orderNo, out string error)
        {
            TabulateData data;

            return CancelOrder(code, orderNo, out data, out error);
        }

        public bool CancelOrder(string code, int orderNo, out TabulateData result, out string error)
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

            TdxWrapper.CancelOrder(ClientId, exchange.ExchangeId.ToString(), orderNo.ToString(), resultInfo, errorInfo);

            error = errorInfo.ToString();
            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultInfo.ToString());
            }

            return succeeded;
        }

        public bool[] CancelOrder(string[] codes, int[] orderNos, out string[] errors)
        {
            TabulateData[] data;

            return CancelOrder(codes, orderNos, out data, out errors);
        }

        public bool[] CancelOrder(string[] codes, int[] orderNos, out TabulateData[] results, out string[] errors)
        {
            CheckDisposed();
            CheckLoggedOn();

            if (codes == null || codes.Length == 0 || orderNos == null || orderNos.Length != codes.Length)
            {
                throw new ArgumentNullException();
            }

            IntPtr[] resultInfos = AllocateStringBuffers(codes.Length, MaxResultStringSize);
            IntPtr[] errorInfos = AllocateStringBuffers(codes.Length, MaxErrorStringSize);

            try
            {
                var exchangeIds = codes.Select(c => Exchange.GetTradeableExchangeForSecurity(c))
                    .Select(e => e == null ? string.Empty : e.ExchangeId.ToString())
                    .ToArray();
                var orderNoStrings = orderNos.Select(id => id.ToString()).ToArray();

                TdxWrapper.CancelOrders(
                    ClientId,
                    exchangeIds,
                    orderNoStrings,
                    codes.Length,
                    resultInfos,
                    errorInfos);

                string[] resultStrings = ConvertStringBufferToString(resultInfos);
                errors = ConvertStringBufferToString(errorInfos);

                bool[] succeeds = new bool[codes.Length];
                results = new TabulateData[codes.Length];

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

        public IEnumerable<QueryGeneralOrderResult> QuerySubmittedOrderHistory(DateTime startDate, DateTime endDate, out string error)
        {
            TabulateData data;
            if (!QueryHistoryData(HistoryDataCategory.OrderSubmittedInHistory, startDate, endDate, out data, out error))
            {
                return null;
            }

            return QueryGeneralOrderResult.ExtractFrom(data);
        }

        public IEnumerable<QuerySucceededOrderResult> QuerySucceededOrderHistory(DateTime startDate, DateTime endDate, out string error)
        {
            TabulateData data;
            if (!QueryHistoryData(HistoryDataCategory.OrderSucceededInHistory, startDate, endDate, out data, out error))
            {
                return null;
            }

            return QuerySucceededOrderResult.ExtractFrom(data);
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
