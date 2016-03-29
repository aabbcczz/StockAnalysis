namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    public delegate void SendAccumulatedMessageDelegate(MultiMessagePackage message, int tag);

    public abstract class MultiMessagePackageAccumulator
    {
        private Stopwatch _watch;
        private int _tag;
        private int _capacity = int.MaxValue;
        private int _accumulatedCount = 0;
        private long _maxAccumulationTicks = 0;
        private long _accumulatedTimeStart = 0;
        private MultiMessagePackage _container = null;
        private object _syncRoot = new object();

        protected MultiMessagePackageAccumulator(int tag, int capacity, int maxAccumulationMilliseconds)
        {
            OnSendAccumulatedMessage = null;

            _watch = new Stopwatch();
            _watch.Start();

            if (capacity > 0)
            {
                _capacity = capacity;
            }

            if (maxAccumulationMilliseconds > 0)
            {
                _maxAccumulationTicks = Stopwatch.Frequency * maxAccumulationMilliseconds / 1000;
            }

            _tag = tag;
        }

        public SendAccumulatedMessageDelegate OnSendAccumulatedMessage { get; set; }

        public void AddMessage(MessagePackage message)
        {
            MultiMessagePackage package = null;
            lock (_syncRoot)
            {
                if (_container == null)
                {
                    CreateContainer();
                }

                _container.AddMessage(message);
                _accumulatedCount++;

                if (_accumulatedCount == 1 && _maxAccumulationTicks > 0)
                {
                    _accumulatedTimeStart = _watch.ElapsedTicks;
                }

                if (_accumulatedCount >= _capacity
                    || (_maxAccumulationTicks > 0
                        && (_watch.ElapsedTicks - _accumulatedTimeStart >= _maxAccumulationTicks)))
                {
                    package = _container;
                    CreateContainer();
                }
            }

            // move send() out of lock to reduce contention
            if (package != null)
            {
                Send(package);
            }
        }

        public void Flush()
        {
            MultiMessagePackage package = null;
            lock (_syncRoot)
            {
                if (_accumulatedCount > 0)
                {
                    package = _container;
                    CreateContainer();
                }
            }

            if (package != null)
            {
                Send(package);
            }
        }

        protected abstract MultiMessagePackage CreatePackage();

        private void CreateContainer()
        {
            _container = CreatePackage();
            _accumulatedCount = 0;
        }

        private void Send(MultiMessagePackage package)
        {
            if (OnSendAccumulatedMessage != null)
            {
                OnSendAccumulatedMessage(package, _tag);
            }
        }
    }
}
