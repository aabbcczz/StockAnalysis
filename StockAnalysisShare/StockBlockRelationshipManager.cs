using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class StockBlockRelationshipManager
    {
        // map from stock to blocks that the stock belongs to.
        private Dictionary<string, string[]> _blocksOfStock = new Dictionary<string, string[]>();

        // map from block to stocks that the block contains
        private Dictionary<string, string[]> _stocksOfBlock = new Dictionary<string, string[]>();

        public StockBlockRelationshipManager(IEnumerable<StockBlockRelationship> relationships)
        {
            _blocksOfStock = relationships
                .GroupBy(sbr => sbr.StockCode)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(sbr => sbr.BlockName)
                        .OrderBy(s => s)
                        .ToArray());

            _stocksOfBlock = relationships
                .GroupBy(sbr => sbr.BlockName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(sbr => sbr.StockCode)
                        .OrderBy(s => s)
                        .ToArray());
        }

        public IEnumerable<string> Stocks
        {
            get { return _blocksOfStock.Keys; }
        }

        public IEnumerable<string> Blocks
        {
            get { return _stocksOfBlock.Keys; }
        }

        public IEnumerable<string> GetBlocksForStock(string stock)
        {
            return _blocksOfStock[stock];
        }

        public IEnumerable<string> GetStocksInBlock(string block)
        {
            return _stocksOfBlock[block];
        }
    }
}
