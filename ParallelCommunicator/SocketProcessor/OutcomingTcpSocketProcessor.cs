namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using FastRank;

    internal class OutcomingTcpSocketProcessor : TcpSocketProcessor
    {
        #region fields and properties

        private bool _disposed = false;

        #endregion

        #region constructors

        public OutcomingTcpSocketProcessor(TcpClient client)
            : base(client)
        {
        }

        #endregion

        public override void SendData(
            IList<ArraySegment<byte>> dataBuffers,
            DataSentCallback callback,
            object state)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }

            if (dataBuffers == null)
            {
                throw new ArgumentNullException("dataBuffers");
            }

            PackageHeader header;
            header.MagicNumber = PackageHeader.StandardMagicNumber;
            header.SerialNumber = PackageHeader.GetSerialNumber();
            header.RawDataLength = dataBuffers.Sum(x => x.Count);
            header.Flags = PackageHeader.PackageFlags.None;
            header.CompressedDataLength = header.RawDataLength; // do not support compress now

            // allocate header buffer. the buffer will be returned in SendCallback after whole data are sent out
            ArraySegment<byte> headerBuffer = AllocateNetworkBuffer(PackageHeader.SizeOfHeader);
            int offset = headerBuffer.Offset;
            header.ToByteArray(headerBuffer.Array, ref offset);

            List<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>(1 + dataBuffers.Count);
            buffers.Add(headerBuffer);
            buffers.AddRange(dataBuffers);

            TcpSocketAsyncEventArgs args = TcpSocketAsyncEventArgs.Allocate();

            try
            {
#if PACKET_DEBUG
                Console.WriteLine("[PKG] {3:hh:mm:ss:ffff} {0:X8} SI {1:X8} {2}", GetHashCode(), header.SerialNumber, Client.Client.RemoteEndPoint, DateTime.Now);
#endif
#if BUFFER_DEBUG
                DumpBuffers("[SI]", dataBuffers, header.SerialNumber);
#endif
                args.HeaderBuffer = headerBuffer;
                args.SetBuffer(null, 0, 0); // ensure not bufferlist and buffer both non-null;
                args.BufferList = buffers;
                args.SendDataBuffer = new List<ArraySegment<byte>>(buffers); // save a copy because arg.BufferList might be updated in sending
                args.RemainingDataLength = headerBuffer.Count + header.RawDataLength;
                args.DataSentCallback = callback;
                args.DataSentState = state;
                args.Header = header;
                args.TimerObj = FastRank.Timer.Time(TimerEvent.DataSend);
                args.IOCompleted += OnSendCompleted;

                BeginSend(args);
            }
            catch (Exception ex)
            {
                HandleException(ex, args);

                throw;
            }
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                base.Dispose(disposing);
            }

            _disposed = true;
        }

        #endregion

        private static void FreeResourcesAndCallback(TcpSocketAsyncEventArgs args)
        {
            try
            {
                if (args.DataSentCallback != null)
                {
                    args.DataSentCallback(args.DataSentState);
                }
            }
            finally
            {
                if (args.HeaderBuffer.Array != null)
                {
                    FreeNetworkBuffer(args.HeaderBuffer);
                }

                TcpSocketAsyncEventArgs.Free(args);
            }
        }

        private void HandleException(Exception ex, TcpSocketAsyncEventArgs args)
        {
            EndPoint remoteEndPoint = Client.Client.RemoteEndPoint;

            FreeResourcesAndCallback(args);
            Shutdown();

            OnException(ex, remoteEndPoint);
        }

        private bool HandleEvent(TcpSocketAsyncEventArgs args, out bool handled)
        {
            handled = false;

            if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
            {
                HandleException(new SocketException((int)args.SocketError), args);

                return false;
            }
            else
            {
                if (args.BytesTransferred < args.RemainingDataLength)
                {
                    int remainingBytes = args.BytesTransferred;
                    while (remainingBytes > 0)
                    {
                        if (args.BufferList[0].Count <= remainingBytes)
                        {
                            remainingBytes -= args.BufferList[0].Count;
                            args.BufferList.RemoveAt(0);
                        }
                        else
                        {
                            ArraySegment<byte> buffer = args.BufferList[0];

                            args.BufferList[0] = new ArraySegment<byte>(
                                buffer.Array,
                                buffer.Offset + remainingBytes,
                                buffer.Count - remainingBytes);

                            remainingBytes = 0;
                        }
                    }

                    args.RemainingDataLength -= args.BytesTransferred;
                }
                else
                {
                    ProcessSentData(args);

                    handled = true;
                }
            }

            return true;
        }

        private void BeginSend(TcpSocketAsyncEventArgs args)
        {
            try
            {
                bool handled = false;

                while (!Client.Client.SendAsync(args))
                {
                    if (!HandleEvent(args, out handled))
                    {
                        break;
                    }

                    if (handled)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, args);

                throw;
            }
        }

        private void OnSendCompleted(object sender, TcpSocketAsyncEventArgs args)
        {
            try
            {
                bool handled = false;
                if (HandleEvent(args, out handled))
                {
                    if (!handled)
                    {
                        BeginSend(args);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, args);

                throw;
            }
        }

        private void ProcessSentData(TcpSocketAsyncEventArgs args)
        {
            if (args.TimerObj != null)
            {
                IDisposable timer = (IDisposable)args.TimerObj;
                timer.Dispose();
            }

#if PACKET_DEBUG
            Console.WriteLine("[PKG] {3:hh:mm:ss:ffff} {0:X8} SX {1:X8} {2}", GetHashCode(), args.Header.SerialNumber, Client.Client.RemoteEndPoint, DateTime.Now);
#endif

#if BUFFER_DEBUG
            var dataBuffers = new List<ArraySegment<byte>>(args.SendDataBuffer);
            dataBuffers.RemoveAt(0); // remove package header buffer.
            DumpBuffers("[SX]", dataBuffers, args.Header.SerialNumber);
#endif

            // call handlers
            OnSentData(args.SendDataBuffer);

            FreeResourcesAndCallback(args);
        }
    }
}
