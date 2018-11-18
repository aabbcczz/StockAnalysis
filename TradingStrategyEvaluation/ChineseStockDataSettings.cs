namespace StockAnalysis.TradingStrategy.Evaluation
{
    using System;
    using System.Xml.Serialization;
    using System.IO;
    using Common.SymbolName;

    [Serializable]
    public sealed class ChineseStockDataSettings
    {
        public const string StockSymbolPattern = "%c";

        public string StockNameTableFile { get; set; }

        public string StockDataFileDirectory { get; set; }

        public string StockDataFileNamePattern { get; set; }

        public static ChineseStockDataSettings LoadFromFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            ChineseStockDataSettings settings;

            var serializer = new XmlSerializer(typeof(ChineseStockDataSettings));

            using (var reader = new StreamReader(file))
            {
                settings = (ChineseStockDataSettings)serializer.Deserialize(reader);
            }

            if (String.IsNullOrEmpty(settings.StockNameTableFile)
                || String.IsNullOrEmpty(settings.StockDataFileDirectory)
                || String.IsNullOrEmpty(settings.StockDataFileNamePattern))
            {
                throw new InvalidDataException("Empty field is not allowed");
            }

            if (settings.StockDataFileNamePattern.IndexOf(StockSymbolPattern, StringComparison.Ordinal) < 0)
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

            var serializer = new XmlSerializer(typeof(ChineseStockDataSettings));

            using (var writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        public string BuildActualDataFilePathAndName(string symbol)
        {
            return Path.Combine(
                StockDataFileDirectory,
                StockDataFileNamePattern.Replace(StockSymbolPattern, StockName.GetNormalizedSymbol(symbol)));
        }

        public static ChineseStockDataSettings GenerateExampleSettings()
        {
            var settings = new ChineseStockDataSettings
            {
                StockDataFileDirectory = @"d:\stock\",
                StockDataFileNamePattern = @"%c.day.csv",
                StockNameTableFile = @"stocknames.txt"
            };

            return settings;
        }
    }
}
