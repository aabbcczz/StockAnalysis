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
        private Dictionary<string, string[]> _stockToBlocksMap = new Dictionary<string, string[]>();

        // map from block to stocks that the block contains
        private Dictionary<string, string[]> _blockToStocksMap = new Dictionary<string, string[]>();

        public StockBlockRelationshipManager(IEnumerable<StockBlockRelationship> relationships)
        {
            _stockToBlocksMap = relationships
                .GroupBy(sbr => sbr.StockCode)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(sbr => sbr.BlockName)
                        .OrderBy(s => s)
                        .ToArray());

            _blockToStocksMap = relationships
                .GroupBy(sbr => sbr.BlockName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(sbr => sbr.StockCode)
                        .OrderBy(s => s)
                        .ToArray());
        }

        public IEnumerable<string> Stocks
        {
            get { return _stockToBlocksMap.Keys; }
        }

        public IEnumerable<string> Blocks
        {
            get { return _blockToStocksMap.Keys; }
        }

        public IEnumerable<string> GetBlocksForStock(string stock)
        {
            return _stockToBlocksMap[stock];
        }

        public IEnumerable<string> GetStocksInBlock(string block)
        {
            return _blockToStocksMap[block];
        }

        public StockBlockRelationshipManager CreateSubsetForStocks(IEnumerable<string> stocks)
        {
            var relationships = stocks
                .Where(_stockToBlocksMap.ContainsKey)
                .SelectMany(
                    s => _stockToBlocksMap[s]
                        .Select(
                            b => new StockBlockRelationship() 
                            { 
                                StockCode = s, 
                                BlockName = b 
                            }));

            return new StockBlockRelationshipManager(relationships);
        }

        public StockBlockRelationshipManager CreateSubsetForBlocks(IEnumerable<string> blocks)
        {
            var relationships = blocks
                .Where(_blockToStocksMap.ContainsKey)
                .SelectMany(
                    b => _blockToStocksMap[b]
                        .Select(
                            s => new StockBlockRelationship()
                            {
                                StockCode = s,
                                BlockName = b
                            }));

            return new StockBlockRelationshipManager(relationships);
        }

        /// <summary>
        /// Find out the mininum set of stocks that can cover all blocks and 
        /// ensure each block has at least <paramref name="minStockPerBlock"/> stocks
        /// </summary>
        /// <param name="minStockPerBlock">the mininum number of stocks in each block</param>
        /// <returns>the set of stocks that can cover all blocks</returns>
        /// <remarks>if a block has less number of stocks than <paramref name="minStockPerBlock"/>, 
        /// all stocks in the block will be selected</remarks>
        public string[] FindMinimumStockSetCoveredAllBlocks(int minStockPerBlock)
        {
            if (minStockPerBlock <= 0)
            {
                throw new ArgumentOutOfRangeException("the mininum number of stocks in each block must be greater than 0");
            }

            string[] stocks = _stockToBlocksMap.Keys.ToArray();
            string[] blocks = _blockToStocksMap.Keys.ToArray();

            var stockIndices = Enumerable
                .Range(0, stocks.Length)
                .ToDictionary(i => stocks[i], i => i);

            var blockIndices = Enumerable
                .Range(0, blocks.Length)
                .ToDictionary(i => blocks[i], i => i);

            // build relationship matrix, each row is stock and each column is block.
            var relationshipMatrix = new int[stocks.Length][];
            for (int i = 0; i < relationshipMatrix.Length; ++i)
            {
                var row = new int[blocks.Length];

                foreach (var block in _stockToBlocksMap[stocks[i]])
                {
                    row[blockIndices[block]] = 1;
                }

                relationshipMatrix[i] = row;
            }

            // build stock counter and block counter
            var stockCounter = Enumerable
                .Range(0, stocks.Length)
                .Select(i => _stockToBlocksMap[stocks[i]].Count())
                .ToArray();

            var blockCounter = Enumerable
                .Range(0, blocks.Length)
                .Select(i => minStockPerBlock)
                .ToArray();

            // algorithm: 
            // while there is still block whose counter is not zero, 
            //      select the stock with the largest counter (>0) (if there are more than one stock has the largest counter, select the stock with smaller index). 
            //      For selected stock, 
            //          set its counter to 0
            //          remove all "1" in the row in relationship matrix
            //          reduce 1 from the counter of blocks which contains the stock.
            //          if a block counter is reduced to 0, the counter of stocks in the block will be reduced by one

            var selectedStocks = new List<string>();
            while (blockCounter.Any(i => i > 0))
            {
                // find the stock with largest counter (>0)
                int maxStockCounter = 0;
                int maxStockCounterIndex = 0;
                for (int i = 0; i < stockCounter.Length; ++i)
                {
                    if (stockCounter[i] > maxStockCounter)
                    {
                        maxStockCounter = stockCounter[i];
                        maxStockCounterIndex = i;
                    }
                }

                if (maxStockCounter == 0)
                {
                    // no more stock to select.
                    break;
                }

                // add the stock to selected stocks
                selectedStocks.Add(stocks[maxStockCounterIndex]);

                // set the count for the stock to 0, means it has been selected, 
                // and it should not participated in the process any more.
                stockCounter[maxStockCounterIndex] = 0;

                // reduce the count of blocks by one for the blocks that contain the selected stock.
                var row = relationshipMatrix[maxStockCounterIndex];
                for (int blockIndex = 0; blockIndex < row.Length; ++blockIndex)
                {
                    if (row[blockIndex] > 0)
                    {
                        // clear the relationship between stock and block
                        row[blockIndex] = 0;

                        // reduce the count of blocks that contain the selected stock
                        if (blockCounter[blockIndex] > 0)
                        {
                            --blockCounter[blockIndex];

                            // if a block count is reduced to zero, it means all stocks to cover this block has been selected,
                            // so all other stocks in the block should not be considered for this block, thus the counter of
                            // those stocks should be reduced by one.
                            if (blockCounter[blockIndex] == 0)
                            {
                                for (int stockIndex = 0; stockIndex < relationshipMatrix.Length; ++stockIndex)
                                {
                                    if (relationshipMatrix[stockIndex][blockIndex] > 0)
                                    {
                                        relationshipMatrix[stockIndex][blockIndex] = 0;
                                        if (stockCounter[stockIndex] > 0)
                                        {
                                            --stockCounter[stockIndex];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return selectedStocks.ToArray();
        }
    }
}
