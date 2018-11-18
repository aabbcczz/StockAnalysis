namespace StockAnalysis.StockTrading.Utility
{
    using System;
    using System.Text;

    public sealed class TdxTradingServer : ITradingServer
    {
        public TdxTradingServer()
        {
        }

        public int Logon(string IP, short port, string version, short yybId, string accountNo, string tradeAccount, string tradePassword, string communicationPassword, out string error)
        {
            StringBuilder errorBuilder = new StringBuilder(TradingHelper.MaxErrorStringSize);

            int clientId = TdxWrapper.Logon(IP, port, version, yybId, accountNo, tradeAccount, tradePassword, communicationPassword, errorBuilder);

            error = errorBuilder.ToString();

            return clientId;
        }

        public void Logoff(int clientId)
        {
            TdxWrapper.Logoff(clientId);
        }

        public void QueryData(int clientId, int category, out string result, out string error)
        {
            StringBuilder resultBuilder = new StringBuilder(TradingHelper.MaxResultStringSize);
            StringBuilder errorBuilder = new StringBuilder(TradingHelper.MaxErrorStringSize);

            TdxWrapper.QueryData(clientId, category, resultBuilder, errorBuilder);

            result = resultBuilder.ToString();
            error = errorBuilder.ToString();
        }

        public void SendOrder(int clientId, int category, int priceType, string shareholderCode, string securitySymbol, float price, int quantity, out string result, out string error)
        {
            StringBuilder resultBuilder = new StringBuilder(TradingHelper.MaxResultStringSize);
            StringBuilder errorBuilder = new StringBuilder(TradingHelper.MaxErrorStringSize);

            TdxWrapper.SendOrder(clientId, category, priceType, shareholderCode, securitySymbol, price, quantity, resultBuilder, errorBuilder);

            result = resultBuilder.ToString();
            error = errorBuilder.ToString();
        }

        public void CancelOrder(int clientId, string exchangeId, string orderNo, out string result, out string error)
        {
            StringBuilder resultBuilder = new StringBuilder(TradingHelper.MaxResultStringSize);
            StringBuilder errorBuilder = new StringBuilder(TradingHelper.MaxErrorStringSize);

            TdxWrapper.CancelOrder(clientId, exchangeId, orderNo, resultBuilder, errorBuilder);

            result = resultBuilder.ToString();
            error = errorBuilder.ToString();
        }

        public void GetQuote(int clientId, string securitySymbol, out string result, out string error)
        {
            StringBuilder resultBuilder = new StringBuilder(TradingHelper.MaxResultStringSize);
            StringBuilder errorBuilder = new StringBuilder(TradingHelper.MaxErrorStringSize);

            TdxWrapper.GetQuote(clientId, securitySymbol, resultBuilder, errorBuilder);

            result = resultBuilder.ToString();
            error = errorBuilder.ToString();
        }

        public void QueryHistoryData(int clientId, int category, string startDate, string endDate, out string result, out string error)
        {
            StringBuilder resultBuilder = new StringBuilder(TradingHelper.MaxResultStringSize);
            StringBuilder errorBuilder = new StringBuilder(TradingHelper.MaxErrorStringSize);

            TdxWrapper.QueryData(clientId, category, resultBuilder, errorBuilder);

            result = resultBuilder.ToString();
            error = errorBuilder.ToString();
        }


        public void QueryData(int clientId, int[] categories, int categoryCount, out string[] results, out string[] errors)
        {
            IntPtr[] resultPtrs = null;
            IntPtr[] errorPtrs = null;

            try
            {
                resultPtrs = TradingHelper.AllocateStringBuffers(categoryCount, TradingHelper.MaxResultStringSize);
                errorPtrs = TradingHelper.AllocateStringBuffers(categoryCount, TradingHelper.MaxErrorStringSize);

                TdxWrapper.QueryDatas(clientId, categories, categoryCount, resultPtrs, errorPtrs);

                results = TradingHelper.ConvertStringBufferToString(resultPtrs);
                errors = TradingHelper.ConvertStringBufferToString(errorPtrs);
            }
            finally
            {
                if (resultPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(resultPtrs);
                }

                if (errorPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(errorPtrs);
                }
            }
        }

        public void SendOrders(int clientId, int[] categories, int[] priceTypes, string[] shareholderCodes, string[] securitySymbols, float[] prices, int[] quantities, int orderCount, out string[] results, out string[] errors)
        {
            IntPtr[] resultPtrs = null;
            IntPtr[] errorPtrs = null;

            try
            {
                resultPtrs = TradingHelper.AllocateStringBuffers(orderCount, TradingHelper.MaxResultStringSize);
                errorPtrs = TradingHelper.AllocateStringBuffers(orderCount, TradingHelper.MaxErrorStringSize);

                TdxWrapper.SendOrders(clientId, categories, priceTypes, shareholderCodes, securitySymbols, prices, quantities, orderCount, resultPtrs, errorPtrs);

                results = TradingHelper.ConvertStringBufferToString(resultPtrs);
                errors = TradingHelper.ConvertStringBufferToString(errorPtrs);
            }
            finally
            {
                if (resultPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(resultPtrs);
                }

                if (errorPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(errorPtrs);
                }
            }
        }

        public void CancelOrders(int clientId, string[] exchangeIds, string[] orderNoes, int orderCount, out string[] results, out string[] errors)
        {
            IntPtr[] resultPtrs = null;
            IntPtr[] errorPtrs = null;

            try
            {
                resultPtrs = TradingHelper.AllocateStringBuffers(orderCount, TradingHelper.MaxResultStringSize);
                errorPtrs = TradingHelper.AllocateStringBuffers(orderCount, TradingHelper.MaxErrorStringSize);

                TdxWrapper.CancelOrders(clientId, exchangeIds, orderNoes, orderCount, resultPtrs, errorPtrs);

                results = TradingHelper.ConvertStringBufferToString(resultPtrs);
                errors = TradingHelper.ConvertStringBufferToString(errorPtrs);
            }
            finally
            {
                if (resultPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(resultPtrs);
                }

                if (errorPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(errorPtrs);
                }
            }
        }

        public void GetQuotes(int clientId, string[] securitySymbols, int securityCount, out string[] results, out string[] errors)
        {
            IntPtr[] resultPtrs = null;
            IntPtr[] errorPtrs = null;

            try
            {
                resultPtrs = TradingHelper.AllocateStringBuffers(securityCount, TradingHelper.MaxResultStringSize);
                errorPtrs = TradingHelper.AllocateStringBuffers(securityCount, TradingHelper.MaxErrorStringSize);

                TdxWrapper.GetQuotes(clientId, securitySymbols, securityCount, resultPtrs, errorPtrs);

                results = TradingHelper.ConvertStringBufferToString(resultPtrs);
                errors = TradingHelper.ConvertStringBufferToString(errorPtrs);
            }
            finally
            {
                if (resultPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(resultPtrs);
                }

                if (errorPtrs != null)
                {
                    TradingHelper.FreeStringBuffers(errorPtrs);
                }
            }
        }
    }
}
