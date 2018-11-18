namespace DataAccess
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using StockAnalysis.Common.Utility;
    using StockAnalysis.Common.SymbolName;

    internal static class FileStorageAccessorHelper
    {
        /// <summary>
        /// Get a relative path to the partition data file
        /// /// </summary>
        /// <param name="description">partition description</param>
        /// <returns>relative path to the data folder based on data scription</returns>
        public static string GetPartitionDataRelativePath(DataPartitionDescription description, SecuritySymbol symbol)
        {
            if (description == null || symbol == null)
            {
                throw new ArgumentNullException();
            }

            if (description.DataDescription == null)
            {
                throw new ArgumentNullException();
            }

            string path = Path.Combine(
                description.DataDescription.Category.ToString(), 
                description.DataDescription.RepricingRight.ToString(), 
                description.DataDescription.Schema.ToString());

            if (description.DataDescription.Schema == DataSchema.Bar || description.DataDescription.Schema == DataSchema.Dde)
            {
                path = Path.Combine(path, GetGranularityString(description.DataDescription.Granularity));
            }

            path = Path.Combine(path, GetSymbolHashPrefix(symbol.NormalizedSymbol, 2));

            return ConvertStringToValidPath(path);
        }

        /// <summary>
        /// Get data file name based on symbol and partition information
        /// </summary>
        /// <param name="symbol">symbol of security whose data to be accessed</param>
        /// <param name="description">partition description</param>
        /// <returns>corresponding data file name</returns>
        public static string GetPartitionDataFileName(DataPartitionDescription description, SecuritySymbol symbol)
        {
            if (symbol == null || description == null)
            {
                throw new ArgumentNullException();
            }

            var fileName = $"{symbol.NormalizedSymbol}.{description.PartitionId}.tea";

            return ConvertStringToValidFileName(fileName);
        }

        /// <summary>
        /// Get an absolute path to the data file based on data description and base path
        /// </summary>
        /// <param name="basePath">base path</param>
        /// <param name="description">data description</param>
        /// <param name="symbol">symbol of security whose data to be accessed</param>
        /// <returns>the absolute path to the data folder</returns>
        public static string GetPartitionDataAbsolutePath(string rootPath, DataPartitionDescription description, SecuritySymbol symbol)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentNullException();
            }

            if (description == null || symbol == null)
            {
                throw new ArgumentNullException();
            }

            if (description.DataDescription == null)
            {
                throw new ArgumentNullException();
            }

            if (!Path.IsPathRooted(rootPath))
            {
                throw new ArgumentException($"Path {rootPath} is not rooted");
            }

            var path = Path.Combine(
                rootPath, 
                GetPartitionDataRelativePath(description, symbol), 
                GetPartitionDataFileName(description, symbol));
            
            path = ConvertStringToValidPath(path);

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Convert granularity to string 
        /// </summary>
        /// <param name="granularity">data granularity</param>
        /// <returns>string representation of granularity</returns>
        public static string GetGranularityString(uint granularity)
        {
            if (granularity == 0)
            {
                throw new ArgumentOutOfRangeException("granularity must be greater than 0");
            }

            DataGranularity dataGranularity;

            if (Enum.IsDefined(typeof(DataGranularity), granularity))
            {
                dataGranularity = (DataGranularity)granularity;
                return dataGranularity.ToString();
            }
            else
            {
                return string.Format("D%0s", granularity);
            }
        }

        /// <summary>
        /// Get the hash prefix for give symbol. the prefix can be used to evenly distribute data related to the symbol
        /// </summary>
        /// <param name="symbol">symbol</param>
        /// <param name="prefixLength">length of prefix to return</param>
        /// <returns>hash prefix string</returns>
        public static string GetSymbolHashPrefix(string symbol, int prefixLength)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                throw new ArgumentNullException("symbol");
            }

            if (prefixLength <= 0)
            {
                throw new ArgumentOutOfRangeException("length must be greater than 0");
            }

            var hash = MD5Hash.GetHashString(symbol);

            if (prefixLength > hash.Length)
            {
                prefixLength = hash.Length;
            }

            return hash.Substring(0, prefixLength);
        }

        /// <summary>
        /// Convert string to valid path if it is not valid or keep it unchanged if it is valid
        /// </summary>
        /// <param name="s">string to be checked and converted</param>
        /// <returns>string that can be valid path</returns>
        public static string ConvertStringToValidPath(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            var invalidPathChars = Path.GetInvalidPathChars();

            if (s.IndexOfAny(invalidPathChars) < 0)
            {
                return s;
            }

            var ss = s.Select(c =>
                     {
                        if (invalidPathChars.Contains(c))
                        {
                            return ConvertInvalidCharInPathOrFileNameToString(c);
                        }
                        else
                        {
                            return new string(c, 1);
                        }
                    });

            // don't use string.Concat here because it will cause IntelliTest not working
            return string.Join(string.Empty, ss);
        }

        /// <summary>
        /// Convert string to valid file name if it is not valid or keep it unchanged if it is valid
        /// </summary>
        /// <param name="s">string to be checked and converted</param>
        /// <returns>string that can be valid file name</returns>
        public static string ConvertStringToValidFileName(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }


            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            if (s.IndexOfAny(invalidFileNameChars) < 0)
            {
                return s;
            }

            var ss = s.Select(c =>
            {
                if (invalidFileNameChars.Contains(c))
                {
                    return ConvertInvalidCharInPathOrFileNameToString(c);
                }
                else
                {
                    return new string(c, 1);
                }
            });

            // don't use string.Concat here because it will cause IntelliTest not working
            return string.Join(string.Empty, ss);
        }

        private static string ConvertInvalidCharInPathOrFileNameToString(char c)
        {
            var bytes = Encoding.UTF8.GetBytes(new char[] { c });
            StringBuilder sb = new StringBuilder();

            sb.Append("_");
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            sb.Append("_");

            return sb.ToString();
        }
    }
}
