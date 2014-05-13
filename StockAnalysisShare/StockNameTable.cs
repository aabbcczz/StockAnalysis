using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StockAnalysis.Share
{
    public sealed class StockNameTable
    {
        private List<StockName> _stockNames = new List<StockName>();

        public IEnumerable<StockName> StockNames { get { return _stockNames; } }

        public int Count { get { return _stockNames.Count; } }

        public StockNameTable()
        {
        }

        public void AddStock(StockName stock)
        {
            _stockNames.Add(stock);
        }

        /// <summary>
        /// Create stock name table from file
        /// </summary>
        /// <param name="fileName">file that contains the stock names. 
        /// The file must be UTF-8 encoded, and each line is formated as:
        /// code|name1|name2|...
        /// </param>
        /// <param name="onError">function to handle invalid input line in the file. default value is null.
        /// if the value is null, exception will be thrown out when invalid input line is encounted, 
        /// otherwise, the invalid input line will be passed to the function.
        /// 
        /// if the function returns false, no more input lines will be read from file.
        /// </param>
        public StockNameTable(string fileName, Func<string, bool> onError = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                HashSet<string> existingCodes = new HashSet<string>();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    StockName stockName = null;
                    try
                    {
                        stockName = new StockName(line);

                        // avoid duplicated stock name (two stocks are treated as duplicated iff. their code are the same)
                        if (existingCodes.Add(stockName.Code))
                        {
                            _stockNames.Add(stockName);
                        }
                    }
                    catch
                    {
                        if (onError == null)
                        {
                            throw;
                        }
                        else
                        {
                            bool continueProcess = onError(line);
                            if (!continueProcess)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
