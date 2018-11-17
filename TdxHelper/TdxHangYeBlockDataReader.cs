namespace StockAnalysis.TdxHelper
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using Common.ChineseMarket;
    using Common.SymbolName;

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
                StockSymbol = StockName.GetNormalizedSymbol(fields[1]),
                BlockName = block.Name
            };
        }
    }
}
