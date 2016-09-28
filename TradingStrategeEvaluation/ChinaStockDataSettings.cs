using System;
using System.Xml.Serialization;
using System.IO;
using StockAnalysis.Share;

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

            var serializer = new XmlSerializer(typeof(ChinaStockDataSettings));

            using (var reader = new StreamReader(file))
            {
                settings = (ChinaStockDataSettings)serializer.Deserialize(reader);
            }

            if (String.IsNullOrEmpty(settings.StockNameTableFile)
                || String.IsNullOrEmpty(settings.StockDataFileDirectory)
                || String.IsNullOrEmpty(settings.StockDataFileNamePattern))
            {
                throw new InvalidDataException("Empty field is not allowed");
            }

            if (settings.StockDataFileNamePattern.IndexOf(StockCodePattern, StringComparison.Ordinal) < 0)
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

            var serializer = new XmlSerializer(typeof(ChinaStockDataSettings));

            using (var writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        public string BuildActualDataFilePathAndName(string code)
        {
            return Path.Combine(
                StockDataFileDirectory,
                StockDataFileNamePattern.Replace(StockCodePattern, StockName.GetCanonicalCode(code)));
        }

        public static ChinaStockDataSettings GenerateExampleSettings()
        {
            var settings = new ChinaStockDataSettings
            {
                StockDataFileDirectory = @"d:\stock\",
                StockDataFileNamePattern = @"%c.day.csv",
                StockNameTableFile = @"stocknames.txt"
            };

            return settings;
        }
    }
}
