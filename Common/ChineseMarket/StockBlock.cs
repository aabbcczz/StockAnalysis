namespace StockAnalysis.Common.ChineseMarket
{
    using System.Collections.Generic;

    /// <summary>
    /// 股票板块
    /// </summary>
    public sealed class StockBlock
    {
        private List<StockBlock> _children = null;

        /// <summary>
        /// name of block. unique, can't be null or empty
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// ID of block. optional, will be unique if it is not null or empty.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Parent block, could be null.
        /// </summary>
        public StockBlock ParentBlock { get; private set; }

        public IEnumerable<StockBlock> ChildBlocks
        {
            get { return _children; }
        }

        public StockBlock(string name, string id = "")
        {
            Name = name;
            Id = id;
        }

        public void AddChild(StockBlock childBlock)
        {
            if (_children == null)
            {
                _children = new List<StockBlock>();
            }

            _children.Add(childBlock);
            childBlock.ParentBlock = this;
        }

        public void RemoveChild(StockBlock childBlock)
        {
            if (_children == null)
            {
                return;
            }

            int index = -1;
            for (int i = 0; i < _children.Count; ++i)
            {
                if (object.ReferenceEquals(_children[i], childBlock))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                _children.RemoveAt(index);
                if (_children.Count == 0)
                {
                    _children = null;
                }

                childBlock.ParentBlock = null;
            }
        }

        public void RemoveAllChildren()
        {
            if (_children == null)
            {
                return;
            }

            foreach (var child in _children)
            {
                child.ParentBlock = null;
            }

            _children = null;
        }
    }
}
