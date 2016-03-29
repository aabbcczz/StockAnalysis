namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class RepeatableQueue<T>
    {
        private List<T> _items = new List<T>();
        private int _currentPosition = 0;

        public void Clear()
        {
            _items.Clear();
            _currentPosition = 0;
        }

        public void Reset()
        {
            _currentPosition = 0;
        }

        public void Enqueue(T item)
        {
            _items.Add(item);
        }

        public void EnqueueRange(IEnumerable<T> collection)
        {
            _items.AddRange(collection);
        }

        public bool TryDequeue(out T item)
        {
            item = default(T);
            while (true)
            {
                int currentPos = _currentPosition;

                if (currentPos >= _items.Count)
                {
                    return false;
                }

                if (Interlocked.CompareExchange(ref _currentPosition, currentPos + 1, currentPos) == currentPos) 
                {
                    // success
                    item = _items[currentPos];
                    return true;
                }
            }
        }
    }
}
