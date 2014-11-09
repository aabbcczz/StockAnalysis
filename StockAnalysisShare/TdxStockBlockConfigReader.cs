using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class TdxStockBlockConfigReader
    {
        private const int FieldCount = 6;

        private static char[] _splitter = new char[] { '|' };

        private List<StockBlock> _blocks = new List<StockBlock>();

        public IEnumerable<StockBlock> Blocks
        {
            get { return _blocks; }
        }

        public TdxStockBlockConfigReader(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException();
            }

            var lines = File.ReadAllLines(file, Encoding.GetEncoding("GB2312"));

            foreach (var line in lines)
            {
                var stockBlock = ParseLine(line);
                if (stockBlock != null)
                {
                    _blocks.Add(stockBlock);
                }
            }
        }

        private static StockBlock ParseLine(string line)
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

            return new StockBlock(fields[0], fields.Last());
        }
    }
}
