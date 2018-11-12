using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace StockAnalysis.Share
{
    public sealed class TradingObjectNameTable<T>
        where T : ITradingObjectName
    {
        private readonly Dictionary<string, T> _names = new Dictionary<string, T>();

        public IEnumerable<T> Names { get { return _names.Values; } }

        public int Count { get { return _names.Count; } }

        public T this[string symbol]
        {
            get 
            {
                return _names[symbol];
            }
        }

        public TradingObjectNameTable()
        {
        }

        public void AddName(T name)
        {
            _names.Add(name.Symbol.NormalizedSymbol, name);
        }

        public bool ContainsObject(string symbol)
        {
            return _names.ContainsKey(symbol);
        }

        /// <summary>
        /// Create stock name table from file
        /// </summary>
        /// <param name="fileName">file that contains the stock names. 
        /// The file must be UTF-8 encoded, and each line is formated as:
        /// symbol|name1|name2|...
        /// </param>
        /// <param name="onError">function to handle invalid input line in the file. default value is null.
        /// if the value is null, exception will be thrown out when invalid input line is encounted, 
        /// otherwise, the invalid input line will be passed to the function.
        /// 
        /// if the function returns false, no more input lines will be read from file.
        /// </param>
        public static TradingObjectNameTable<T> LoadFromFile(string fileName, Func<string, bool> onError = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            TradingObjectNameTable<T> table = new TradingObjectNameTable<T>();

            using (var reader = new StreamReader(fileName, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        T name = (T)TradingObjectNameFactory.ParseFromString(typeof(T), line);

                        // avoid duplicated stock name (two stocks are treated as duplicated iff. their symbol are the same)
                        if (!table.ContainsObject(name.Symbol.NormalizedSymbol))
                        {
                            table.AddName(name);
                        }
                    }
                    catch
                    {
                        if (onError == null)
                        {
                            throw;
                        }

                        var continueProcess = onError(line);
                        if (!continueProcess)
                        {
                            break;
                        }
                    }
                }
            }

            return table;
        }
    }
}
