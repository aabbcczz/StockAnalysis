using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace TradingStrategyEvaluation
{
    [Serializable]
    public sealed class ChinaStockDataSettings
    {
        public const string StockCodePattern = "%c";

        public string StockNameTableFile { get; set; }

        public string StockDataFileDirectory { get; set; }

        public string StockDataFileNamePattern { get; set; }

        public static ChinaStockDataSettings LoadFromFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            ChinaStockDataSettings settings;

            XmlSerializer serializer = new XmlSerializer(typeof(ChinaStockDataSettings));

            using (StreamReader reader = new StreamReader(file))
            {
                settings = (ChinaStockDataSettings)serializer.Deserialize(reader);
            }

            if (String.IsNullOrEmpty(settings.StockNameTableFile)
                || String.IsNullOrEmpty(settings.StockDataFileDirectory)
                || String.IsNullOrEmpty(settings.StockDataFileNamePattern))
            {
                throw new InvalidDataException("Empty field is not allowed");
            }

            if (settings.StockDataFileNamePattern.IndexOf(StockCodePattern) < 0)
            {
                throw new InvalidDataException("Stock data file name pattern is invalid");
            }

            return settings;
        }

        public void SaveToFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ChinaStockDataSettings));

            using (StreamWriter writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        public string BuildActualDataFilePathAndName(string code)
        {
            return Path.Combine(
                StockDataFileDirectory,
                StockDataFileNamePattern.Replace(StockCodePattern, code));
        }
    }
}
