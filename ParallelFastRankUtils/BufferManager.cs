namespace ParallelFastRank
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FastRank;

    public sealed class BufferManager
    {
        private const int MininumBufferSize = 0x20; // the minumum buffer is 32 bytes.

        private static BufferManager instance = null;
        private static uint inUseFlag = 0xDEADBEEF;
        private static uint notInUseFlag = 0;
        private static int payloadSize = sizeof(int) * 2;

        private BlockPool[] _pools = null;

        static BufferManager()
        {
            BufferManager.instance = new BufferManager();
        }

        private BufferManager()
        {
            _pools = new BlockPool[32];

            int blockSize = BufferManager.MininumBufferSize;
            int index = 0;
            while (blockSize > 0)
            {
                _pools[index] = new BlockPool(blockSize);
                blockSize = blockSize << 1;
                index++;
            }
        }

        public static BufferManager Instance
        {
            get { return instance; }
        }

        public long AllocatedMemorySize
        {
            get { return _pools.Sum(p => (p == null ? 0 : p.AllocatedMemorySize)); }
        }

        public ArraySegment<byte> TakeBuffer(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            int index = GetPoolIndex(size + BufferManager.payloadSize);

            ArraySegment<byte> block = _pools[index].TakeBlock();

            ArraySegment<byte> result = BufferManager.SavePayload(block, size);

#if BUFFER_DEBUG
            Console.WriteLine(
                "Take buffer {0:X8}, offset {1}, size {2}, actual block offset {3} size {4}", 
                result.Array.GetHashCode(), 
                result.Offset, 
                result.Count, 
                block.Offset, 
                block.Count);
#endif
            return result;
        }

        public void ReturnBuffer(ArraySegment<byte> buffer)
        {
            ArraySegment<byte> block = BufferManager.RestorePayload(buffer);

#if BUFFER_DEBUG
            Console.WriteLine(
                "Return buffer {0:X8}, offset {1}, size {2}, actual block offset {3} size {4}", 
                buffer.Array.GetHashCode(), 
                buffer.Offset, 
                buffer.Count, 
                block.Offset, 
                block.Count);
#endif

            int index = GetPoolIndex(block.Count);
            _pools[index].ReturnBlock(block);
        }

        public void ReturnBuffers(IList<ArraySegment<byte>> buffers)
        {
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }

            foreach (var buffer in buffers)
            {
                ReturnBuffer(buffer);
            }
        }
        
        private static int GetPoolIndex(int size)
        {
            System.Diagnostics.Debug.Assert(size > 0, "buffer size is not valid");

            int index = 0;
            int blockSize = BufferManager.MininumBufferSize;

            while (blockSize > 0 && blockSize < size)
            {
                blockSize = blockSize << 1;
                index++;
            }

            return index;
        }

        private static ArraySegment<byte> SavePayload(ArraySegment<byte> block, int expectedSize)
        {
            System.Diagnostics.Debug.Assert(
                block.Count >= expectedSize + BufferManager.payloadSize,
                "allocated buffer is not large enough");

            // save payload (real buffer size and in use flag) at begining of block.
            int offset = block.Offset;
            block.Count.ToByteArray(block.Array, ref offset);

#if DEBUG
            // check if buffer is in using
            int newOffset = offset;
            uint inUseFlag = block.Array.ToUInt(ref newOffset);
            if (inUseFlag != BufferManager.notInUseFlag)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Buffer {0:X8}, offset {1}, size {2} is used", 
                        block.Array.GetHashCode(), 
                        block.Offset, 
                        block.Count));
            }
#endif
            BufferManager.inUseFlag.ToByteArray(block.Array, ref offset);

            return new ArraySegment<byte>(block.Array, block.Offset + BufferManager.payloadSize, expectedSize);
        }

        private static ArraySegment<byte> RestorePayload(ArraySegment<byte> block)
        {
            int offset = block.Offset - BufferManager.payloadSize;
            int count = block.Array.ToInt(ref offset);

#if DEBUG
            // check if buffer is not in using
            int newOffset = offset;
            uint inUseFlag = block.Array.ToUInt(ref newOffset);
            if (inUseFlag != BufferManager.inUseFlag)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Buffer {0:X8}, offset {1}, size {2} is not used", 
                        block.Array.GetHashCode(), 
                        block.Offset, 
                        block.Count));
            }
#endif

            BufferManager.notInUseFlag.ToByteArray(block.Array, ref offset);

            System.Diagnostics.Debug.Assert(
                count >= block.Count + BufferManager.payloadSize,
                "allocated buffer is not large enough"); 
            
            return new ArraySegment<byte>(block.Array, block.Offset - BufferManager.payloadSize, count);
        }
    }
}
