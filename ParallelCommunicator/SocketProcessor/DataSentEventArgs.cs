namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class DataSentEventArgs : EventArgs
    {
        public DataSentEventArgs(IList<ArraySegment<byte>> buffers)
        {
            SentDataBuffers = buffers;
        }

        public IList<ArraySegment<byte>> SentDataBuffers { get; private set; }
    }
}
