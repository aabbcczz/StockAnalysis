using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using StockAnalysis.Share;

namespace StockTrading.Utility
{
    public sealed class TradingClient : IDisposable
    {
        private Dictionary<IExchange, string> _shareholderCodes = new Dictionary<IExchange, string>();

        public int ClientId { get; private set; }

        public ITradingServer Server { get; private set; }

        public TradingClient(ITradingServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException();
            }

            ClientId = TradingHelper.InvalidClientId;

            Server = server;
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
            error = string.Empty;

            if (IsLoggedOn())
            {
                throw new InvalidOperationException("Client has logged on");
            }

            int clientId = Server.Logon(
                address,
                port,
                protocolVersion,
                yybId,
                accountNo,
                tradeAccount,
                tradePassword,
                communicationPassword,
                out error);

            if (clientId >= 0)
            {
                ClientId = clientId;

                if (!InitializeAfterLoggedOn(tradeAccount, out error))
                {
                    LogOff();

                    error = "Failed to initialize client after logged on. " + error;
                }
            }

            return IsLoggedOn();
        }

        private bool InitializeAfterLoggedOn(string tradeAccount, out string error)
        {
            error = string.Empty;

            var registries = QueryShareholderRegistry(out error);

            if (registries == null)
            {
                return false;
            }

            registries = registries.Where(r => r.CapitalAccount == tradeAccount);
            if (registries.Count() == 0)
            {
                error = "No shareholder registry can match trade account";
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
            return ClientId != TradingHelper.InvalidClientId;
        }
    
        public void LogOff()
        {
            if (!IsLoggedOn())
            {
                return;
            }

            TdxWrapper.Logoff(ClientId);

            ClientId = TradingHelper.InvalidClientId;
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
            CheckLoggedOn();

            string resultString;

            Server.QueryData(ClientId, (int)category, out resultString, out error);

            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultString);
            }

            return  succeeded;
        }

        public bool[] QueryData(DataCategory[] categories, out TabulateData[] results, out string[] errors)
        {
            CheckLoggedOn();

            if (categories == null || categories.Length == 0)
            {
                throw new ArgumentNullException();
            }

            string[] resultStrings;

            int[] categoryArray = categories.Select(c => (int)c).ToArray();
 
            Server.QueryData(ClientId, categoryArray, categoryArray.Length, out resultStrings, out errors);

            bool[] succeeds = new bool[categories.Length];
            results = new TabulateData[categories.Length];

            for (int i = 0; i < results.Length; ++i)
            {
                succeeds[i] = string.IsNullOrEmpty(errors[i]);

                results[i] = succeeds[i] ? TabulateData.Parse(resultStrings[i]) : null;
            }

            return succeeds;
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
            CheckLoggedOn();

            string resultString;

            Server.GetQuote(ClientId, securityCode, out resultString, out error);

            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultString);
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
            CheckLoggedOn();

            if (securityCodes == null || securityCodes.Length == 0)
            {
                throw new ArgumentNullException();
            }

            string[] resultStrings;

            Server.GetQuotes(ClientId, securityCodes, securityCodes.Length, out resultStrings, out errors);

            bool[] succeeds = new bool[securityCodes.Length];
            results = new TabulateData[securityCodes.Length];

            for (int i = 0; i < results.Length; ++i)
            {
                succeeds[i] = string.IsNullOrEmpty(errors[i]);

                results[i] = succeeds[i] ? TabulateData.Parse(resultStrings[i]) : null;
            }

            return succeeds;
        }

        public string GetShareholderCode(IExchange exchange)
        {
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
            IExchange exchange = ExchangeFactory.GetTradingExchangeForSecurity(securityCode);
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
            CheckLoggedOn();

            string shareholderCode = GetShareholderCode(request.SecurityCode);

            result = null;
            string resultString;

            Server.SendOrder(
                ClientId,
                (int)request.Category,
                (int)request.PricingType,
                shareholderCode,
                request.SecurityCode, 
                request.Price,
                request.Volume,
                out resultString, 
                out error);

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultString.ToString());
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
            CheckLoggedOn();

            if (requests == null || requests.Length == 0)
            {
                throw new ArgumentNullException();
            }

            string[] resultStrings;

            var shareholderCodes = requests.Select(req => GetShareholderCode(req.SecurityCode)).ToArray();
            var categories = requests.Select(req => (int)req.Category).ToArray();
            var priceTypes = requests.Select(req => (int)req.PricingType).ToArray();
            var securityCodes = requests.Select(req => req.SecurityCode).ToArray();
            var prices = requests.Select(req => req.Price).ToArray();
            var quantities = requests.Select(req => req.Volume).ToArray();

            Server.SendOrders(
                ClientId,
                categories,
                priceTypes,
                shareholderCodes,
                securityCodes,
                prices,
                quantities,
                requests.Count(),
                out resultStrings,
                out errors);
                    
            bool[] succeeds = new bool[securityCodes.Length];
            results = new TabulateData[securityCodes.Length];

            for (int i = 0; i < results.Length; ++i)
            {
                succeeds[i] = string.IsNullOrEmpty(errors[i]);

                results[i] = succeeds[i] ? TabulateData.Parse(resultStrings[i]) : null;
            }

            return succeeds;
        }

        public bool CancelOrder(string code, int orderNo, out string error)
        {
            TabulateData data;

            return CancelOrder(code, orderNo, out data, out error);
        }

        public bool CancelOrder(string code, int orderNo, out TabulateData result, out string error)
        {
            CheckLoggedOn();

            string resultString;

            IExchange exchange = ExchangeFactory.GetTradingExchangeForSecurity(code);
            if (exchange == null)
            {
                result = null;
                error = "Invalid code";
                return false;
            }

            Server.CancelOrder(ClientId, exchange.ExchangeId.ToString(), orderNo.ToString(), out resultString, out error);

            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultString);
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
            CheckLoggedOn();

            if (codes == null || codes.Length == 0 || orderNos == null || orderNos.Length != codes.Length)
            {
                throw new ArgumentNullException();
            }

            string[] resultStrings;


            var exchangeIds = codes.Select(c => ExchangeFactory.GetTradingExchangeForSecurity(c))
                .Select(e => e == null ? string.Empty : e.ExchangeId.ToString())
                .ToArray();
            var orderNoStrings = orderNos.Select(id => id.ToString()).ToArray();

            Server.CancelOrders(
                ClientId,
                exchangeIds,
                orderNoStrings,
                codes.Length,
                out resultStrings,
                out errors);

            bool[] succeeds = new bool[codes.Length];
            results = new TabulateData[codes.Length];

            for (int i = 0; i < results.Length; ++i)
            {
                succeeds[i] = string.IsNullOrEmpty(errors[i]);

                results[i] = succeeds[i] ? TabulateData.Parse(resultStrings[i]) : null;
            }

            return succeeds;
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
            CheckLoggedOn();

            string resultString;

            Server.QueryHistoryData(
                ClientId, 
                (int)category, 
                startDate.ToString("yyyyMMdd"), 
                endDate.ToString("yyyyMMdd"),
                out resultString, 
                out error);

            result = null;

            bool succeeded = string.IsNullOrEmpty(error);

            if (succeeded)
            {
                result = TabulateData.Parse(resultString);
            }

            return succeeded;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (IsLoggedOn())
                {
                    LogOff();
                }

                disposedValue = true;
            }
        }

        ~TradingClient()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
