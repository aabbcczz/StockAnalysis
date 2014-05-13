using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using StockAnalysis.Share;

namespace GetFinanceReports
{
    public sealed class WebReportFetcher : IReportFetcher
    {
        public const string DefaultSuffixOfOutputFile = @"html";

        public string FinanceReportServerAddress { get; private set; }

        public WebReportFetcher(string serverAddress)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
            {
                throw new ArgumentNullException("server");
            }

            FinanceReportServerAddress = serverAddress;
        }

        public string GetDefaultSuffixOfOutputFile()
        {
            return DefaultSuffixOfOutputFile;
        }

        public bool FetchReport(StockName stock, string outputFile, out string errorMessage)
        {
            if (stock == null)
            {
                throw new ArgumentNullException("stock");
            }

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new ArgumentNullException("outputFile");
            }

            string address = ReportServerAddressFormatter.Format(FinanceReportServerAddress, stock, ReportServerAddressFormatter.DefaultAbbrevationMarketFormatter);

            return FetchReport(address, stock, outputFile, out errorMessage);
        }

        private static bool FetchReport(string serverAddress, StockName stock, string outputFile, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        errorMessage = string.Format("HTTP Status {0} for {1}", response.StatusCode, serverAddress);
                        return false;
                    }

                    string body = null;
                    string charset = response.CharacterSet;
                    Encoding encoding = string.IsNullOrWhiteSpace(charset) ? Encoding.UTF8 : Encoding.GetEncoding(charset);

                    // save the response body to memory stream to avoid send web request twice if 
                    // the character set specified in HTTP header is different with what specified in body.
                    using (MemoryStream bodyStream = new MemoryStream())
                    {
                        response.GetResponseStream().CopyTo(bodyStream);

                        bodyStream.Seek(0, SeekOrigin.Begin);
                    
                        // do not use "using" or close the reader to avoid the memory stream being closed.
                        StreamReader sr = new StreamReader(bodyStream, encoding); 
                        body = sr.ReadToEnd();

                        // Check real charset meta-tag in HTML
                        const string meta = "charset=";
                        int charsetStart = body.IndexOf(meta);
                        if (charsetStart > 0)
                        {
                            charsetStart += meta.Length;
                            int charsetEnd = body.IndexOfAny(new[] { ' ', '\"', ';' }, charsetStart);
                            string realCharset = body.Substring(charsetStart, charsetEnd - charsetStart);

                            // real charset meta-tag in HTML differs from supplied server header???
                            if (realCharset != response.CharacterSet)
                            {
                                // get correct encoding
                                encoding = Encoding.GetEncoding(realCharset);

                                // reset stream position to beginning
                                bodyStream.Seek(0, SeekOrigin.Begin);

                                // reread response stream with the correct encoding
                                sr = new StreamReader(bodyStream, encoding);

                                body = sr.ReadToEnd();
                            }
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
                    {
                        writer.Write(body);
                    }
                }
            }
            catch (UriFormatException ex)
            {
                errorMessage = ex.ToString();
                return false;
            }
            catch (WebException ex)
            {
                errorMessage = ex.ToString();
                return false;
            }
            catch (IOException ex)
            {
                errorMessage = ex.ToString();
                return false;
            }

            return true;
        }
    }
}
