namespace ParallelFastRank
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using FastRank;

    /// <summary>
    /// This class is a wrapper of outcoming tcp socket processor that allow multiple threads
    /// to send message safely.
    /// </summary>
    internal sealed class ThreadSafeOutcomingTcpSocketProcessor : SocketProcessorBase
    {
        private bool _disposed = false;

        private OutcomingTcpSocketProcessor _processor = null;

        public ThreadSafeOutcomingTcpSocketProcessor()
            : base()
        {
        }

        /// <summary>
        /// Initialize the socket processor.
        /// </summary>
        /// <param name="localMachineName">local machine name or ip</param>
        /// <param name="remoteMachineName">remote machine name or ip</param>
        /// <param name="remotePort">port listened by remote machine</param>
        /// <param name="connectionTryTimes">the time of retrying to connect to remote machine</param>
        /// <param name="connectionRetryIntervalInSeconds">the interval in second between two retries of connection</param>
        public void Initialize(
            string localMachineName, 
            string remoteMachineName, 
            int remotePort,
            int connectionTryTimes = 1,
            int connectionRetryIntervalInSeconds = 60)
        {
            TcpClient client = null;

            try
            {
                connectionTryTimes = Math.Max(1, connectionTryTimes);

                while (connectionTryTimes > 0)
                {
                    try
                    {
                        client = TcpSocketCommunicationChannel.CreateTcpClientBindToLocal(
                            localMachineName,
                            remoteMachineName,
                            remotePort);
                        break;
                    }
                    catch (SocketException ex)
                    {
                        StaticRuntimeContext.Stderr.WriteLine("Create TcpClient failed: {0}", ex.ToString());
                        StaticRuntimeContext.Stderr.WriteLine(
                            "local machine name : {0}, remote machine name: {1}, remote port: {2}",
                            localMachineName,
                            remoteMachineName,
                            remotePort);

                        connectionTryTimes--;

                        if (connectionTryTimes == 0)
                        {
                            throw;
                        }

                        // sleep some time before next retry
                        Thread.Sleep(connectionRetryIntervalInSeconds * 1000);
                    }
                }

                _processor = new OutcomingTcpSocketProcessor(client);
                _processor.SentDataEvent += OnProcessorSentData;
                _processor.ExceptionEvent += OnProcessorException;
            }
            catch
            {
                if (client != null)
                {
                    client.Close();
                }

                throw;
            }
        }

        public override void SendData(
            IList<ArraySegment<byte>> buffers,
            DataSentCallback callback,
            object state)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }

            lock (_processor)
            {
                _processor.SendData(buffers, callback, state);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _processor.Close();
                }

                base.Dispose(disposing);
            }

            _disposed = true;
        }
        
        private void OnProcessorSentData(object sender, DataSentEventArgs args)
        {
            // call sent message event handler.
            OnSentData(args.SentDataBuffers);
        }

        private void OnProcessorException(object sender, DataProcessExceptionEventArgs args)
        {
            OnException(args.Exception, args.Context);
        }
    }
}