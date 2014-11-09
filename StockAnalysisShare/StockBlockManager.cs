using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class StockBlockManager
    {
        private const string HierachicalBlockIdHead = "T";

        private Dictionary<string, StockBlock> _blockNameIndices = new Dictionary<string, StockBlock>();
        private Dictionary<string, StockBlock> _blockIdIndices = new Dictionary<string, StockBlock>();

        public IEnumerable<string> GetAllBlockNames()
        {
            return _blockNameIndices.Keys;
        }

        public IEnumerable<StockBlockRelationship> ExpandRelationships(IEnumerable<StockBlockRelationship> relationships)
        {
            if (relationships == null)
            {
                throw new ArgumentNullException();
            }

            foreach (var relationship in relationships)
            {
                foreach (var expandedRelationship in ExpandRelationship(relationship))
                {
                    yield return expandedRelationship;
                }
            }
        }

        public IEnumerable<StockBlockRelationship> ExpandRelationship(StockBlockRelationship relationship)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException();
            }

            yield return relationship;

            StockBlock block;
            if (_blockNameIndices.TryGetValue(relationship.BlockName, out block))
            {
                while (block.ParentBlock != null)
                {
                    yield return new StockBlockRelationship
                    {
                        StockCode = relationship.StockCode,
                        BlockName = block.ParentBlock.Name
                    };

                    block = block.ParentBlock;
                }
            }
        }

        public StockBlock GetStockBlockByName(string name)
        {
            StockBlock block = null;

            _blockNameIndices.TryGetValue(name, out block);

            return block;
        }

        public StockBlock GetStockBlockById(string id)
        {
            StockBlock block = null;
            _blockIdIndices.TryGetValue(id, out block);

            return block;
        }

        public void AddStockBlock(StockBlock block)
        {
            if (block == null)
            {
                throw new ArgumentNullException();
            }

            StockBlock oldBlock;
            if (_blockNameIndices.TryGetValue(block.Name, out oldBlock))
            {
                if (block.Id.StartsWith(HierachicalBlockIdHead))
                {
                    RemoveStockBlock(oldBlock.Name);
                }
                else
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(block.Id))
            {
                if (_blockIdIndices.ContainsKey(block.Id))
                {
                    throw new InvalidOperationException(string.Format("StockBlock with id {0} exists", block.Id));
                }
            }

            _blockNameIndices.Add(block.Name, block);

            if (!string.IsNullOrEmpty(block.Id))
            {
                _blockIdIndices.Add(block.Id, block);

                // build the hierarchy of block
                var children = FindChildren(block.Id);
                if (children != null && children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        block.AddChild(child);
                    }
                }

                var parent = FindParent(block.Id);
                if (parent != null)
                {
                    parent.AddChild(block);
                }
            }
        }

        public void AddStockBlocks(IEnumerable<StockBlock> blocks)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException();
            }

            foreach (var block in blocks)
            {
                AddStockBlock(block);
            }
        }

        private void RemoveStockBlock(string blockName)
        {
            if (string.IsNullOrEmpty(blockName))
            {
                throw new ArgumentNullException();
            }

            // find the block firstly
            StockBlock block;
            if (!_blockNameIndices.TryGetValue(blockName, out block))
            {
                return;
            }

            // detach parent/child relationships
            block.RemoveAllChildren();

            if (block.ParentBlock != null)
            {
                block.ParentBlock.RemoveChild(block);
            }

            // now we can remove the block
            _blockNameIndices.Remove(blockName);

            if (!string.IsNullOrEmpty(block.Id))
            {
                _blockIdIndices.Remove(block.Id);
            }
        }

        private List<StockBlock> FindChildren(string parentId)
        {
            if (!parentId.StartsWith(HierachicalBlockIdHead))
            {
                return null;
            }

            if (parentId.Length == 3 || parentId.Length == 5) // Txx or Txxxx, first or second level block
            {
                List<StockBlock> blocks = new List<StockBlock>();

                foreach (var block in _blockIdIndices.Values)
                {
                    if (!string.IsNullOrEmpty(block.Id)
                        && block.Id.StartsWith(parentId)
                        && block.Id.Length == parentId.Length + 2)
                    {
                        blocks.Add(block);
                    }
                }

                return blocks;
            }
            else
            {
                return null;
            }
        }

        private StockBlock FindParent(string childId)
        {
            if (!childId.StartsWith(HierachicalBlockIdHead))
            {
                return null;
            }

            if (childId.Length == 5 || childId.Length == 7) // Txxxx or Txxxxxx, second or third level block
            {
                StockBlock block = null;
                if (_blockIdIndices.TryGetValue(childId.Substring(0, childId.Length - 2), out block))
                {
                    return block;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
