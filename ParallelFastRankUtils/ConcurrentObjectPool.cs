namespace ParallelFastRank
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    
    public sealed class ConcurrentObjectPool<T> where T : class
    {
        private const int DefaultAllocationUnitSize = 10;

#if DEBUG
        private HashSet<T> _allocatedObjectTrace = new HashSet<T>();
        private object _traceLock = new object();
#endif

        private ConcurrentStack<T> _objects = null;
        private Func<object, T> _creator = null;
        private object _creationArg = null;
        private int _allocationUnitSize = DefaultAllocationUnitSize;
        private object _allocationLock = new object();
        private int _capacity = int.MaxValue;
        private int _allocatedObjectCount = 0;

        /// <summary>
        /// constructe an object pool
        /// </summary>
        public ConcurrentObjectPool(
            Func<object, T> creator, 
            object creationArg, 
            int capacity = int.MaxValue, 
            int allocationUnitSize = 0)
        {
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException("capacity must be greater than 0");
            }

            if (allocationUnitSize == 0)
            {
                allocationUnitSize = DefaultAllocationUnitSize;
            }

            _creator = creator;
            _creationArg = creationArg;
            _capacity = capacity;
            _allocationUnitSize = allocationUnitSize;
            _objects = new ConcurrentStack<T>();
        }

        /// <summary>
        /// construct an object pool
        /// </summary>
        public ConcurrentObjectPool(
            Func<T> creator,
            int capacity = int.MaxValue,
            int segmentSize = 0)
            : this((object obj) => { return creator(); }, null, capacity, segmentSize)
        {
        }

        public int AllocationUnitSize
        {
            get
            {
                return _allocationUnitSize;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _allocationUnitSize = value;
            }
        }

        public int ObjectCount
        {
            get { return Interlocked.CompareExchange(ref _allocatedObjectCount, 0, 0); }
        }
        
        /// <summary>
        /// Take an object from pool. 
        /// </summary>
        /// <returns>object in pool. if there is no freed object in the pool and the allocated
        /// objects exceeds pool's capacity, null will be returned</returns>
        public T TakeObject()
        {
            T obj = default(T);
            while (!_objects.TryPop(out obj))
            {
                lock (_allocationLock)
                {
                    // try to get object again to avoid another thread having allocated objects
                    if (_objects.TryPop(out obj))
                    {
                        break;
                    }

                    if (_allocatedObjectCount >= _capacity)
                    {
                        return null;
                    }

                    AllocateObjects();
                }
            }

#if DEBUG
            lock (_traceLock)
            {
                if (_allocatedObjectTrace.Contains(obj))
                {
                    throw new InvalidOperationException("object has not been returned to pool before allocation");
                }

                _allocatedObjectTrace.Add(obj);
            }
#endif
            return obj;
        }

        public void ReturnObject(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

#if DEBUG
            lock (_traceLock)
            {
                if (!_allocatedObjectTrace.Contains(obj))
                {
                    throw new InvalidOperationException("object has not been allocated before returned to pool");
                }

                _allocatedObjectTrace.Remove(obj);
            }
#endif
            _objects.Push(obj);
        }

        private void AllocateObjects()
        {
            int allocationSize = Math.Min(_allocationUnitSize, _capacity - _allocatedObjectCount);

            if (allocationSize < 0)
            {
                throw new InvalidOperationException(
                    "allocation size is smaller than zero, it should be logic error.");
            }

            for (int i = 0; i < allocationSize; i++)
            {
                T obj = _creator(_creationArg);
                _objects.Push(obj);
            }

            _allocatedObjectCount += allocationSize;
        }
    }
}
