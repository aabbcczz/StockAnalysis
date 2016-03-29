namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FastRank;

    // called when a message is sent
    public delegate void DataSentCallback(object state);

    // called when a message is received
    public delegate void DataReceivedCallback(object state);

    public abstract class SocketProcessorBase : IDisposable
    {
        public event EventHandler<DataReceivedEventArgs> ReceivedDataEvent = null;

        public event EventHandler<DataSentEventArgs> SentDataEvent = null;
        
        public event EventHandler<DataProcessExceptionEventArgs> ExceptionEvent = null;

        // send raw data out.
        public abstract void SendData(
            IList<ArraySegment<byte>> messageBuffers,
            DataSentCallback callback,
            object state);

        public void Close()
        {
            Dispose();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        protected static ArraySegment<byte> AllocateNetworkBuffer(int size)
        {
#if NO_BUFFER_MANAGER
            return new ArraySegment<byte>(new byte[size], 0, size);
#else
            return BufferManager.Instance.TakeBuffer(size);
#endif
        }

        protected static void FreeNetworkBuffer(ArraySegment<byte> buffer)
        {
#if NO_BUFFER_MANAGER
            // do nothing
#else
            BufferManager.Instance.ReturnBuffer(buffer);
#endif
        }

        protected virtual void OnReceivedData(
            ArraySegment<byte> dataBuffer,
            DataReceivedCallback callback,
            object state)
        {
            if (ReceivedDataEvent != null)
            {
                DataReceivedEventArgs arg = new DataReceivedEventArgs()
                {
                    ReceivedDataBuffer = dataBuffer,
                    ReceivedDataCallback = callback,
                    CallbackState = state,
                    Timer = FastRank.Timer.Time(TimerEvent.DataWaitForDeserialization)
                };

                ReceivedDataEvent(this, arg);
            }
        }

        protected virtual void OnSentData(IList<ArraySegment<byte>> dataBuffers)
        {
            if (SentDataEvent != null)
            {
                SentDataEvent(this, new DataSentEventArgs(dataBuffers));
            }
        }

        protected virtual void OnException(Exception ex, object context)
        {
            if (ExceptionEvent != null)
            {
                try
                {
                    ExceptionEvent(this, new DataProcessExceptionEventArgs() { Exception = ex, Context = context });
                }
                catch (Exception e)
                {
                    // exit if exception is thrown in exception handler.
                    StaticRuntimeContext.Stderr.WriteLine("Exception occurred in exception handler: {0}", e);
                    Environment.Exit(-1);

                    throw;
                }
            }
            else
            {
                StaticRuntimeContext.Stderr.WriteLine("Exception is caugth: {0}, context: {1}", ex, context);

                throw ex;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
