using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public sealed class TdxTradingServerSimulator : ITradingServer
    {
        private ITradingServer _trueServer;

        public TdxTradingServerSimulator()
        {
            _trueServer = new TdxTradingServer();
        }

        public void CancelOrder(int clientId, string exchangeId, string orderNo, out string result, out string error)
        {
            throw new NotImplementedException();
        }

        public void CancelOrders(int clientId, string[] exchangeIds, string[] orderNoes, int orderCount, out string[] results, out string[] errors)
        {
            results = new string[orderCount];
            errors = new string[orderCount];

            for (int i = 0; i < orderCount; ++i)
            {
                string result;
                string error;

                CancelOrder(clientId, exchangeIds[i], orderNoes[i], out result, out error);

                results[i] = result;
                errors[i] = error;
            }
        }

        public void GetQuote(int clientId, string securityCode, out string result, out string error)
        {
            _trueServer.GetQuote(clientId, securityCode, out result, out error);
        }

        public void GetQuotes(int clientId, string[] securityCodes, int securityCount, out string[] results, out string[] errors)
        {
            _trueServer.GetQuotes(clientId, securityCodes, securityCount, out results, out errors);
        }

        public void Logoff(int clientId)
        {
            _trueServer.Logoff(clientId);
        }

        public int Logon(string IP, short port, string version, short yybId, string accountNo, string tradeAccount, string tradePassword, string communicationPassword, out string error)
        {
            return _trueServer.Logon(IP, port, version, yybId, accountNo, tradeAccount, tradePassword, communicationPassword, out error);
        }

        public void QueryData(int clientId, int category, out string result, out string error)
        {
            throw new NotImplementedException();
        }

        public void QueryData(int clientId, int[] categories, int categoryCount, out string[] results, out string[] errors)
        {
            results = new string[categoryCount];
            errors = new string[categoryCount];

            for (int i = 0; i < categoryCount; ++i)
            {
                string result;
                string error;

                QueryData(clientId, categories[i], out result, out error);

                results[i] = result;
                errors[i] = error;
            }
        }

        public void QueryHistoryData(int clientId, int category, string startDate, string endDate, out string result, out string error)
        {
            _trueServer.QueryHistoryData(clientId, category, startDate, endDate, out result, out error);
        }

        public void SendOrder(int clientId, int category, int priceType, string shareholderCode, string securityCode, float price, int quantity, out string result, out string error)
        {
            result = string.Empty;
            error = string.Empty;

            OrderCategory orderCategory = (OrderCategory)category;

            if (orderCategory != OrderCategory.Buy && orderCategory != OrderCategory.Sell)
            {
                error = string.Format("Unsupported order category {0}", orderCategory);
                return;
            }


        }

        public void SendOrders(int clientId, int[] categories, int[] priceTypes, string[] shareholderCodes, string[] securityCodes, float[] prices, int[] quantities, int orderCount, out string[] results, out string[] errors)
        {
            results = new string[orderCount];
            errors = new string[orderCount];

            for (int i = 0; i < orderCount; ++i)
            {
                string result;
                string error;

                SendOrder(clientId, categories[i], priceTypes[i], shareholderCodes[i], securityCodes[i], prices[i], quantities[i], out result, out error);

                results[i] = result;
                errors[i] = error;
            }
        }
    }
}
