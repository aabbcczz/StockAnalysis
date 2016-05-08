namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    internal sealed class TcpSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        private static ConcurrentObjectPool<TcpSocketAsyncEventArgs> pool
            = new ConcurrentObjectPool<TcpSocketAsyncEventArgs>(() => { return new TcpSocketAsyncEventArgs(); });

        public TcpSocketAsyncEventArgs()
        {
            IOCompleted = null;
        }

        public DataSentCallback DataSentCallback { get; set; } // callback for send

        public object DataSentState { get; set; } // state given by user, it will be passed back after sent data
        
        public IList<ArraySegment<byte>> SendDataBuffer { get; set; }
        
        public int RemainingDataLength { get; set; }
        
        public PackageHeader Header { get; set; } // received or sending header
        
        public bool IsReceivingPackageHeader { get; set; }
        
        public ArraySegment<byte> HeaderBuffer { get; set; } // buffer for sending/receiving header
        
        public ArraySegment<byte> BodyBuffer { get; set; } // buffer for receiving body
        
        public IDisposable TimerObj { get; set; } // used for counting time.

        public EventHandler<TcpSocketAsyncEventArgs> IOCompleted { get; set; }

        public static TcpSocketAsyncEventArgs Allocate()
        {
            TcpSocketAsyncEventArgs args = pool.TakeObject();

            if (args == null)
            {
                throw new InvalidOperationException("allocate TcpSocketAsyncEventArgs failed");
            }

            ((SocketAsyncEventArgs)args).Completed += OnCompleted;
            args.IOCompleted = null;

            return args;
        }

        public static void Free(TcpSocketAsyncEventArgs args)
        {
            ((SocketAsyncEventArgs)args).Completed -= OnCompleted;
            pool.ReturnObject(args);
        }

        private static void OnCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            TcpSocketAsyncEventArgs args = (TcpSocketAsyncEventArgs)eventArgs;

            if (args.IOCompleted != null)
            {
                args.IOCompleted(sender, args);
            }
        }
    }
}
