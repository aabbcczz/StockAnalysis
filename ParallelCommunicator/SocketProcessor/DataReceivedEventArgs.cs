namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class DataReceivedEventArgs : EventArgs
    {
        public ArraySegment<byte> ReceivedDataBuffer { get; set; }

        public DataReceivedCallback ReceivedDataCallback { get; set; }

        public object CallbackState { get; set; }

        public IDisposable Timer { get; set; }
    }
}
