namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using FastRank;

    public abstract class TcpSocketProcessor : SocketProcessorBase
    {
        #region static fields
        private static bool noDelay = true;
        private static int receiveBufferSize = 256 * 1024; // 256K receive buffer
        private static int sendBufferSize = 256 * 1024; // 256K send buffer
        #endregion

        #region fields and properties
        private bool _disposed = false;
        private TcpClient _client = null;
        private Socket _socket = null; // copy of m_client.Client, to avoid null reference when m_client.Close() is called.
        #endregion

        #region constructors

        protected TcpSocketProcessor(TcpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            SetClient(client);
        }

        #endregion

        protected TcpClient Client
        {
            get { return _client; }
            set { SetClient(value); }
        }

        #region static functions
        public static void SetNoDelayFlag(bool noDelay)
        {
            TcpSocketProcessor.noDelay = noDelay;
        }

        public static void SetReceiveBufferSize(int receiveBufferSize)
        {
            TcpSocketProcessor.receiveBufferSize = receiveBufferSize;
        }

        public static void SetSendBufferSize(int sendBufferSize)
        {
            TcpSocketProcessor.sendBufferSize = sendBufferSize;
        }

        #endregion

        protected void Shutdown()
        {
            if (!_disposed)
            {
                try
                {
                    if (_socket != null)
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket = null;
                    }

                    if (_client != null)
                    {
                        StaticRuntimeContext.Stdout.WriteLine(
                            "Shut down socket for client {0}",
                            _client.Client.RemoteEndPoint);
                        _client.Close();
                        _client = null;
                    }
                }
                catch (Exception ex)
                {
                    // ignore any exception when shutdown
                    StaticRuntimeContext.Stderr.WriteLine("Exception in shutting down socket: {0}", ex);
                }
            }
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            StaticRuntimeContext.Stdout1.WriteLine("Dispose TcpSocketProcessor {0:X8}, {1}", GetHashCode(), disposing);
            if (!_disposed)
            {
                if (disposing)
                {
                    Shutdown();
                }

                base.Dispose(disposing);
            }

            _disposed = true;
        }

        #endregion

        private void SetClient(TcpClient tcpClient)
        {
            _client = tcpClient;
            _socket = tcpClient.Client;

            // there are lots of small messages, so we set NoDelay to true to ensure a message will be 
            // sent out immediately especially when there are multiple connections;
            _socket.NoDelay = TcpSocketProcessor.noDelay;
            _socket.ReceiveBufferSize = TcpSocketProcessor.receiveBufferSize;
            _socket.SendBufferSize = TcpSocketProcessor.sendBufferSize;
        }
    }
}
