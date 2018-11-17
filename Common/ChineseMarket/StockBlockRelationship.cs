namespace StockAnalysis.Common.ChineseMarket
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CsvHelper;

    public sealed class StockBlockRelationship
    {
        public string StockSymbol { get; set; }
        public string BlockName { get; set; }

        public static void SaveToFile(string file, IEnumerable<StockBlockRelationship> records)
        {
            using (StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteRecords(records);
                }
            }
        }

        public static IEnumerable<StockBlockRelationship> LoadFromFile(string file)
        {
            List<StockBlockRelationship> records;

            using (StreamReader reader = new StreamReader(file, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    records = csvReader.GetRecords<StockBlockRelationship>().ToList();
                }
            }

            return records;
        }
    }
}
