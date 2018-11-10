using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;

namespace StockAnalysis.Share
{
    public static class SinaStockQuoteInterface
    {
        private const string SinaQuoteWebServiceUriPrefix = "http://hq.sinajs.cn/list=";

        private static char[] lineSplitter = new char[] { '\n' };

        private static string NormalizeCode(string code)
        {
            var exchange = ExchangeFactory.CreateExchangeById(StockName.GetExchangeId(code));

            string prefix = exchange.CapitalizedSymbolPrefix.ToLowerInvariant();

            return prefix + code;
        }

        private static string CreateUriString(string code)
        {
            return SinaQuoteWebServiceUriPrefix + NormalizeCode(code);
        }

        private static string CreateUriString(IEnumerable<string> codes)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var code in codes)
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new ArgumentException("Empty string in input");
                }

                if (builder.Length != 0)
                {
                    builder.Append(",");
                }

                builder.Append(NormalizeCode(code));
            }

            return SinaQuoteWebServiceUriPrefix + builder.ToString();
        }

        private static SinaStockQuote ParseSingleResponseString(string code, string responseString)
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

            return new SinaStockQuote(code, trimedString);
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

        public static async Task<SinaStockQuote> GetQuote(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException();
            }

            string uriString = CreateUriString(code);

            string responseString = await GetResponseString(uriString);

            return ParseSingleResponseString(code, responseString);
        }

        public static async Task<List<SinaStockQuote>> GetQuote(IEnumerable<string> codes)
        {
            if (codes == null)
            {
                throw new ArgumentNullException();
            }

            string uriString = CreateUriString(codes);

            string responseString = await GetResponseString(uriString);

            string[] subStrings = responseString.Split(lineSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (subStrings.Length != codes.Count())
            {
                throw new InvalidOperationException("the number of responses does not match the number of requests");
            }

            int index = 0;
            List<SinaStockQuote> quotes = new List<SinaStockQuote>();
            foreach (var code in codes)
            {
                quotes.Add(ParseSingleResponseString(code, subStrings[index++]));
            }

            return quotes;
        }
    }
}
