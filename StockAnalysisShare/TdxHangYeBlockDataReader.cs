using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    /// <summary>
    /// class for reading out the stock's 行业板块
    /// </summary>
    public sealed class TdxHangYeBlockDataReader
    {
        private const int FieldCount = 4;

        private static char[] _splitter = new char[] { '|' };

        private List<StockBlockRelationship> _relationships = new List<StockBlockRelationship>();

        public IEnumerable<StockBlockRelationship> Relationships
        {
            get { return _relationships; }
        }

        public TdxHangYeBlockDataReader(string file, StockBlockManager blockManager)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException();
            }

            var lines = File.ReadAllLines(file, Encoding.GetEncoding("GB2312"));

            foreach (var line in lines)
            {
                var relationship = ParseLine(line, blockManager);
                if (relationship != null)
                {
                    _relationships.Add(relationship);
                }
            }
        }

        private static StockBlockRelationship ParseLine(string line, StockBlockManager manager)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var fields = line.Split(_splitter);
            if (fields.Length != FieldCount)
            {
                return null;
            }

            StockBlock block = manager.GetStockBlockById(fields[2]);
            if (block == null)
            {
                return null;
            }

            return new StockBlockRelationship
            {
                StockCode = StockName.GetCanonicalCode(fields[1]),
                BlockName = block.Name
            };
        }
    }
}
