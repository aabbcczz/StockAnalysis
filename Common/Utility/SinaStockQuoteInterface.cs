namespace StockAnalysis.Common.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Exchange;
    using SymbolName;

    public static class SinaStockQuoteInterface
    {
        private const string SinaQuoteWebServiceUriPrefix = "http://hq.sinajs.cn/list=";

        private static char[] lineSplitter = new char[] { '\n' };

        private static string NormalizeSymbol(string symbol)
        {
            var exchange = ExchangeFactory.GetExchangeById(StockName.GetExchangeId(symbol));

            string prefix = exchange.CapitalizedSymbolPrefix.ToLowerInvariant();

            return prefix + symbol;
        }

        private static string CreateUriString(string symbol)
        {
            return SinaQuoteWebServiceUriPrefix + NormalizeSymbol(symbol);
        }

        private static string CreateUriString(IEnumerable<string> symbols)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var symbol in symbols)
            {
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    throw new ArgumentException("Empty string in input");
                }

                if (builder.Length != 0)
                {
                    builder.Append(",");
                }

                builder.Append(NormalizeSymbol(symbol));
            }

            return SinaQuoteWebServiceUriPrefix + builder.ToString();
        }

        private static SinaStockQuote ParseSingleResponseString(string symbol, string responseString)
        {
            if (string.IsNullOrEmpty(responseString))
            {
                return null;
            }

            string[] fields = responseString.Split('=');
            if (fields.Length != 2)
            {
                return null;
            }

            // remove quote symbol and comma symbol
            string trimedString = fields[1].Trim(';').Trim('\"');

            if (string.IsNullOrWhiteSpace(trimedString))
            {
                return null;
            }

            return new SinaStockQuote(symbol, trimedString);
        }

        private static async Task<string> GetResponseString(string uriString)
        {
            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync(uriString);

            return response;

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriString);

            //// Set credentials to use for this request.
            //// request.Credentials = CredentialCache.DefaultCredentials;

            //string responseString;

            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{

            //    // Get the stream associated with the response.
            //    Stream receiveStream = response.GetResponseStream();

            //    // Pipes the stream to a higher level stream reader with the required encoding format. 
            //    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding("GB2312")))
            //    {
            //        responseString = readStream.ReadToEnd();
            //    }
            //}

            //return responseString;
        }

        public static async Task<SinaStockQuote> GetQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentNullException();
            }

            string uriString = CreateUriString(symbol);

            string responseString = await GetResponseString(uriString);

            return ParseSingleResponseString(symbol, responseString);
        }

        public static async Task<List<SinaStockQuote>> GetQuote(IEnumerable<string> symbols)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException();
            }

            string uriString = CreateUriString(symbols);

            string responseString = await GetResponseString(uriString);

            string[] subStrings = responseString.Split(lineSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (subStrings.Length != symbols.Count())
            {
                throw new InvalidOperationException("the number of responses does not match the number of requests");
            }

            int index = 0;
            List<SinaStockQuote> quotes = new List<SinaStockQuote>();
            foreach (var symbol in symbols)
            {
                quotes.Add(ParseSingleResponseString(symbol, subStrings[index++]));
            }

            return quotes;
        }
    }
}
