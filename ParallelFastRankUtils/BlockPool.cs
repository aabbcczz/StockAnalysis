namespace ParallelFastRank
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal sealed class BlockPool
    {
        private const int MinSegmentSize = 128 * 1024; // minumum size of a segment of memory. greater than 83K, so we can ensure the buffer is allocated from LOH.
        private const int MinBlockNumberInSegment = 4; // minumum number of blocks in a segment;
        private const int LargeBlockSize = 4 * 1024 * 1024; // blocks larger than this value will be allocated one bye one.

        private int _blockSize = 0;
        private int _segmentSize = 0;
        private int _blockNumberInSegment = 0;
        private object _syncRoot = new object();
        private long _allocatedMemorySize = 0;

#if DEBUG
        private HashSet<byte[]> _segments = new HashSet<byte[]>(); // record all allocated segments.
#endif
        private ConcurrentStack<ArraySegment<byte>> _blocks = new ConcurrentStack<ArraySegment<byte>>();

        public BlockPool(int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize");
            }

            _blockSize = blockSize;

            if (_blockSize >= BlockPool.LargeBlockSize)
            {
                _blockNumberInSegment = 1;
                _segmentSize = _blockSize * _blockNumberInSegment;
            }
            else
            {
                if (_blockSize * BlockPool.MinBlockNumberInSegment < BlockPool.MinSegmentSize)
                {
                    _segmentSize = BlockPool.MinSegmentSize;
                    _blockNumberInSegment = _segmentSize / _blockSize;
                }
                else
                {
                    _segmentSize = _blockSize * BlockPool.MinBlockNumberInSegment;
                    _blockNumberInSegment = BlockPool.MinBlockNumberInSegment;
                }
            }
        }

        public long AllocatedMemorySize
        {
            get { return _allocatedMemorySize; }
        }

        public ArraySegment<byte> TakeBlock()
        {
            ArraySegment<byte> block;

            while (!_blocks.TryPop(out block))
            {
                lock (_syncRoot)
                {
                    // check if any other thread has called AllocateSegment
                    if (_blocks.TryPop(out block))
                    {
                        break;
                    }

                    AllocateSegment();
                }
            }

            return block;
        }

        public void ReturnBlock(ArraySegment<byte> block)
        {
#if DEBUG
            if (block.Count != _blockSize)
            {
                throw new InvalidOperationException("block size is incorrect, the block is not allocated from this pool");
            }

            if (!_segments.Contains(block.Array))
            {
                throw new InvalidOperationException("the block is not allocated from this pool");
            }
#endif
            _blocks.Push(block);
        }

        private void AllocateSegment()
        {
            byte[] buffer = new byte[_segmentSize];
            _allocatedMemorySize += _segmentSize;

#if DEBUG
            _segments.Add(buffer);
#endif

            for (int i = 0; i < _blockNumberInSegment; i++)
            {
                ArraySegment<byte> block = new ArraySegment<byte>(buffer, i * _blockSize, _blockSize);
                _blocks.Push(block);
            }
        }
    }
}
