using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsvHelper;

namespace StockAnalysis.Share
{
    public sealed class StockBlockRelationship
    {
        public string StockCode { get; set; }
        public string BlockName { get; set; }

        public static void SaveToFile(string file, IEnumerable<StockBlockRelationship> records)
        {
            using (StreamWriter writer = new StreamWriter(file, false, Encoding.UTF8))
            {
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    csvWriter.WriteHeader<StockBlockRelationship>();

                    foreach (var record in records)
                    {
                        StockBlockRelationship output = new StockBlockRelationship()
                        {
                            // add "T" before stock code to avoid Excel convert it to number.
                            StockCode = "T" + record.StockCode,
                            BlockName = record.BlockName
                        };

                        csvWriter.WriteRecord(output);
                    }
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

            foreach (var record in records)
            {
                StockBlockRelationship output = new StockBlockRelationship()
                {
                    // remove "T" 
                    StockCode = record.StockCode.Substring(1),
                    BlockName = record.BlockName
                };

                yield return output;
            }
        }
    }
}
