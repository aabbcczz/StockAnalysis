namespace StockAnalysis.Common.Utility
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    
    public sealed class WaitableConcurrentQueue<T> : IDisposable
    {
        private ConcurrentQueue<T> _underlyingQueue;

        private SemaphoreSlim _occupiedItems;

        public WaitableConcurrentQueue()
        {
            _underlyingQueue = new ConcurrentQueue<T>();
            _occupiedItems = new SemaphoreSlim(0);
        }

        public WaitableConcurrentQueue(IEnumerable<T> collection)
        {
            _underlyingQueue = new ConcurrentQueue<T>(collection);
            _occupiedItems = new SemaphoreSlim(collection.Count());
        }
        
        public static T TakeFromAny(
            WaitableConcurrentQueue<T>[] queues,
            CancellationToken token,
            out int queueIndex)
        {
            if (queues == null || queues.Length == 0)
            {
                throw new ArgumentException("queues is null or length is zero");
            }

            T item = default(T);

            // try to take item from any queue firstly.
            for (int i = 0; i < queues.Length; ++i)
            {
                if (queues[i].TryTake(out item))
                {
                    queueIndex = i;
                    return item;
                }
            }

            // now we have to wait.
            WaitHandle[] handles = new WaitHandle[queues.Length + 1];

            for (int i = 0; i < queues.Length; ++i)
            {
                handles[i] = queues[i]._occupiedItems.AvailableWaitHandle;
            }

            handles[handles.Length - 1] = token.WaitHandle;

            SpinWait spinWait = new SpinWait();

            while (true)
            {
                int index = WaitHandle.WaitAny(handles);

                if (index == handles.Length - 1)
                {
                    // cancellation token is triggered
                    throw new OperationCanceledException();
                }
                else
                {
                    if (queues[index].TryTake(out item))
                    {
                        queueIndex = index;
                        break;
                    }
                    else
                    {
                        spinWait.SpinOnce();
                    }
                }
            }

            return item;
        }
        
        public bool TryTake(out T item)
        {
            item = default(T);
            bool gotItem = _underlyingQueue.TryDequeue(out item);

            if (gotItem)
            {
                _occupiedItems.Wait();
            }

            return gotItem;
        }

        public T Take(CancellationToken token)
        {
            T item = default(T);

            bool gotItem = TryTake(out item);

            if (!gotItem)
            {
                SpinWait spinWait = new SpinWait();

                while (true)
                {
                    _occupiedItems.Wait(token);

                    if (_underlyingQueue.TryDequeue(out item))
                    {
                        break;
                    }

                    _occupiedItems.Release();
                    spinWait.SpinOnce();
                }
            }

            return item;
        }

        public void Add(T item)
        {
            _underlyingQueue.Enqueue(item);
            _occupiedItems.Release();
        }

        public void Add(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            int count = 0;
            foreach (T item in items)
            {
                _underlyingQueue.Enqueue(item);
                count++;
            }

            if (count > 0)
            {
                _occupiedItems.Release(count);
            }
        }

        public void RepeatAdd(T item, int repeatTimes)
        {
            for (int i = 0; i < repeatTimes; ++i)
            {
                _underlyingQueue.Enqueue(item);
            }

            if (repeatTimes > 0)
            {
                _occupiedItems.Release(repeatTimes);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_occupiedItems != null)
                {
                    _occupiedItems.Dispose();
                    _occupiedItems = null;
                }
            }
        }
    }
}
