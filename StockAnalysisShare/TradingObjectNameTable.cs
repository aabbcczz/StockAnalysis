using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace StockAnalysis.Share
{
    public sealed class TradingObjectNameTable<T>
        where T : TradingObjectName, new()
    {
        private readonly Dictionary<string, T> _names = new Dictionary<string, T>();

        public IEnumerable<T> Names { get { return _names.Values; } }

        public int Count { get { return _names.Count; } }

        public T this[string code]
        {
            get 
            {
                return _names[code];
            }
        }

        public TradingObjectNameTable()
        {
        }

        public void AddName(T name)
        {
            _names.Add(name.NormalizedCode, name);
        }

        public bool ContainsObject(string code)
        {
            return _names.ContainsKey(code);
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
        public TradingObjectNameTable(string fileName, Func<string, bool> onError = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

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
                        T name = (T)(new T().ParseFromString(line));

                        // avoid duplicated stock name (two stocks are treated as duplicated iff. their code are the same)
                        if (!ContainsObject(name.NormalizedCode))
                        {
                            AddName(name);
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
        }
    }
}
