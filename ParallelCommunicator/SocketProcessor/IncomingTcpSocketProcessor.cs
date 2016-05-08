namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using FastRank;

    internal sealed class IncomingTcpSocketProcessor : TcpSocketProcessor
    {
        #region fields and properties

        private bool _disposed = false;

        #endregion

        #region constructors

        public IncomingTcpSocketProcessor(TcpClient client)
            : base(client)
        {
#if PACKET_DEBUG
            Console.WriteLine("[PKG] {2:hh:mm:ss:ffff} {0:X8} RI PACKAGE HEADER {1}", GetHashCode(), Client.Client.RemoteEndPoint, DateTime.Now);
#endif

            ArraySegment<byte> headerBuffer = AllocateNetworkBuffer(PackageHeader.SizeOfHeader);
            TcpSocketAsyncEventArgs args = TcpSocketAsyncEventArgs.Allocate();

            args.BufferList = null; // ensure not BufferList and Buffer both non-null
            args.SetBuffer(headerBuffer.Array, headerBuffer.Offset, headerBuffer.Count);
            args.HeaderBuffer = headerBuffer;
            args.IsReceivingPackageHeader = true;
            args.TimerObj = null;
            args.IOCompleted += OnReceiveCompleted;

            BeginReceive(args);
        }

        #endregion

        public override void SendData(
            IList<ArraySegment<byte>> dataBuffers,
            DataSentCallback callback,
            object state)
        {
            throw new NotImplementedException();
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

        private static void FreeResources(TcpSocketAsyncEventArgs args)
        {
            if (args.IsReceivingPackageHeader)
            {
                if (args.HeaderBuffer.Array != null)
                {
                    FreeNetworkBuffer(args.HeaderBuffer);
                }
            }
            else
            {
                if (args.BodyBuffer.Array != null)
                {
                    FreeNetworkBuffer(args.BodyBuffer);
                }
            }

            TcpSocketAsyncEventArgs.Free(args);
        }

        private void HandleException(Exception ex, TcpSocketAsyncEventArgs args)
        {
            EndPoint remoteEndPoint = Client.Client.RemoteEndPoint;

            FreeResources(args);
            Shutdown();

            OnException(ex, remoteEndPoint);
        }

        private bool HandleEvent(TcpSocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                if (args.BytesTransferred < args.Count)
                {
                    args.SetBuffer(args.Offset + args.BytesTransferred, args.Count - args.BytesTransferred);

                    return true;
                }
                else
                {
                    return ProcessReceivedData(args);
                }
            }
            else
            {
                // bytes transferred is 0 and/or socket error.

                // backup the error code before free resources.
                SocketError error = args.SocketError;
                EndPoint remoteEndPoint = Client.Client.RemoteEndPoint;

                FreeResources(args);
                Shutdown();

                if (error != SocketError.Success)
                {
                    // not gracefully disconnected
                    OnException(new SocketException((int)error), remoteEndPoint);
                }

                return false;
            }
        }

        private void BeginReceive(TcpSocketAsyncEventArgs args)
        {
            try
            {
                while (!Client.Client.ReceiveAsync(args))
                {
                    if (!HandleEvent(args))
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

        private void OnReceiveCompleted(object sender, TcpSocketAsyncEventArgs args)
        {
            if (_disposed)
            {
                FreeResources(args);

                return;
            }

            try
            {
                if (HandleEvent(args))
                {
                    BeginReceive(args);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, args);

                throw;
            }
        }

        private bool ProcessReceivedData(TcpSocketAsyncEventArgs args)
        {
            try
            {
                if (args.IsReceivingPackageHeader) 
                {
                    // package header is received
                    PackageHeader header = new PackageHeader();

                    int offset = args.HeaderBuffer.Offset;
                    header.FromByteArray(args.HeaderBuffer.Array, ref offset);
                    System.Diagnostics.Debug.Assert(
                        offset == args.HeaderBuffer.Offset + args.HeaderBuffer.Count,
                        "deserialize package header failed");

                    // free header buffer
                    ArraySegment<byte> headerBuffer = args.HeaderBuffer;
                    args.HeaderBuffer = default(ArraySegment<byte>);
                    FreeNetworkBuffer(headerBuffer);

                    if (header.MagicNumber != PackageHeader.StandardMagicNumber)
                    {
#if PACKET_DEBUG
                        Console.WriteLine("[PKG] {0:X8} ERR MAGICNUMBER WRONG {1}", GetHashCode(), Client.Client.RemoteEndPoint);
#endif
                        throw new InvalidMessageException("received package header with wrong magic number");
                    }

#if PACKET_DEBUG
                    Console.WriteLine("[PKG] {3:hh:mm:ss:ffff} {0:X8} RX PACKAGE HEADER {1:X8} {2}", GetHashCode(), header.SerialNumber, Client.Client.RemoteEndPoint, DateTime.Now);
                    Console.WriteLine("[PKG] {4:hh:mm:ss:ffff} {0:X8} RI PACKAGE BODY {1:X8} [length={2}] {3}", GetHashCode(), header.SerialNumber, header.RawDataLength, Client.Client.RemoteEndPoint, DateTime.Now);
#endif

                    if (!_disposed)
                    {
                        args.Header = header;
                        args.BodyBuffer = AllocateNetworkBuffer(header.RawDataLength);
                        args.BufferList = null; // ensure not BufferList and Buffer both non-null
                        args.SetBuffer(args.BodyBuffer.Array, args.BodyBuffer.Offset, args.BodyBuffer.Count);
                        args.TimerObj = FastRank.Timer.Time(TimerEvent.DataReceive);
                        args.IsReceivingPackageHeader = false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else 
                {
                    // recevied body
                    // count time
                    if (args.TimerObj != null)
                    {
                        args.TimerObj.Dispose();
                    }

#if BUFFER_DEBUG
                    DumpBuffers("[RX]", new List<ArraySegment<byte>>() { args.BodyBuffer }, args.Header.SerialNumber);
#endif
#if PACKET_DEBUG
                    Console.WriteLine("[PKG] {3:hh:mm:ss:ffff} {0:X8} RX PACKAGE BODY {1:X8} {2}", GetHashCode(), args.Header.SerialNumber, Client.Client.RemoteEndPoint, DateTime.Now);
                    Console.WriteLine("[PKG] {2:hh:mm:ss:ffff} {0:X8} RI PACKAGE HEADER {1}", GetHashCode(), Client.Client.RemoteEndPoint, DateTime.Now);
#endif
                    using (FastRank.Timer.Time(TimerEvent.OnReceivedData))
                    {
                        ArraySegment<byte> copyOfBodyBuffer = args.BodyBuffer;
                        OnReceivedData(copyOfBodyBuffer, AfterReceivedData, copyOfBodyBuffer);
                    }

                    // free body buffer in AfterReceivedData function. not in here.
                    args.BodyBuffer = default(ArraySegment<byte>);

                    if (!_disposed)
                    {
                        args.HeaderBuffer = AllocateNetworkBuffer(PackageHeader.SizeOfHeader);
                        args.BufferList = null; // ensure not BufferList and Buffer both non-null
                        args.SetBuffer(args.HeaderBuffer.Array, args.HeaderBuffer.Offset, args.HeaderBuffer.Count);
                        args.TimerObj = null;
                        args.IsReceivingPackageHeader = true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex, args);

                throw;
            }
        }

        private void AfterReceivedData(object state)
        {
            ArraySegment<byte> dataBuffer = (ArraySegment<byte>)state;

            FreeNetworkBuffer(dataBuffer);
        }
    }
}
