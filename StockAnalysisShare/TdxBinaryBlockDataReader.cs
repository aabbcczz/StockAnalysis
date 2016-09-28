using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    /// <summary>
    /// class for reading stock block information from TDX block data file. 
    /// data file format refers to http://www.docin.com/p-728707457.html
    /// </summary>
    public sealed class TdxBinaryBlockDataReader
    {
        private const int HeaderSize = 0x180;
        private const int BlockNameSize = 0x9; // 4 chinese character + \0
        private const int StockCodeSize = 0x7; // 6 digits + \0
        private const int MaxNumberOfStockInBlock = 400;

        private static Encoding _stringEncoding = Encoding.GetEncoding("GB2312");

        private List<StockBlockRelationship> _relationships = new List<StockBlockRelationship>();

        public IEnumerable<StockBlockRelationship> Relationships
        {
            get { return _relationships; }
        }

        public TdxBinaryBlockDataReader(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException();
            }

            using (BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                // skip header
                reader.ReadBytes(HeaderSize);

                // read the number of blocks
                int blockNumber = reader.ReadInt16();
                if (blockNumber <= 0)
                {
                    throw new InvalidDataException("block number is not positive number");
                }

                for (int blockIndex = 0; blockIndex < blockNumber; ++blockIndex)
                {
                    // read block name
                    var rawBlockName = reader.ReadBytes(BlockNameSize);
                    var blockName = ConvertRawBytesToString(rawBlockName);

                    // read # of stocks in the block
                    var stockNumber = reader.ReadInt16();

                    if (stockNumber > MaxNumberOfStockInBlock)
                    {
                        throw new InvalidDataException("stock number in block exceeds limit");
                    }

                    // skip the block level
                    reader.ReadInt16();

                    // now read stock code
                    for (int stockIndex = 0; stockIndex < stockNumber; ++stockIndex)
                    {
                        var rawCodes = reader.ReadBytes(StockCodeSize);
                        var stockCode = ConvertRawBytesToString(rawCodes);

                        _relationships.Add(
                            new StockBlockRelationship()
                            {
                                StockCode = StockName.GetCanonicalCode(stockCode),
                                BlockName = blockName
                            });
                    }

                    // skip empty spaces
                    if (stockNumber < MaxNumberOfStockInBlock)
                    {
                        reader.ReadBytes(StockCodeSize * (MaxNumberOfStockInBlock - stockNumber));
                    }
                }
            }
        }

        private static string ConvertRawBytesToString(byte[] bytes)
        {
            return _stringEncoding.GetString(bytes).Trim('\0');
        }
    }
}
