namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// This class is used for collecting message latency for one type of message.
    /// The synchronization of time is taken care by other classes
    /// </summary>
    internal sealed class SingleTypeMessageTimeCollector
    {
        /// <summary>
        /// then maximum number of workers that could send message.
        /// </summary>
        private readonly int _maxWorkerCount;

        /// <summary>
        /// the type of message whose data will be collected.
        /// </summary>
        private readonly MessageType _type;

        private long _messageCount = 0;

        /// <summary>
        /// the number of messages that have negative latency (it is possible due to 
        /// imprecise time synchronization)
        /// </summary>
        private long _negativeTimeMessageCount = 0;

        /// <summary>
        /// message latency performance counter per worker.
        /// </summary>
        private MessageTimeCounter[] _counterByWorker;

        /// <summary>
        /// Initialize the message time collector
        /// </summary>
        /// <param name="type">the type of message to be collected</param>
        /// <param name="maxWorkerCount">maximum number of workers that could send message</param>
        public SingleTypeMessageTimeCollector(MessageType type, int maxWorkerCount)
        {
            _type = type;
            _maxWorkerCount = maxWorkerCount;

            _counterByWorker = new MessageTimeCounter[_maxWorkerCount];
            for (int i = 0; i < _counterByWorker.Length; ++i)
            {
                _counterByWorker[i] = new MessageTimeCounter();
            }
        }

        /// <summary>
        /// the number of messages have been collected
        /// </summary>
        public long MessageCount
        {
            get { return _messageCount; }
        }

        /// <summary>
        /// Collect message data
        /// </summary>
        /// <param name="message">the message whose data to be collected</param>
        /// <param name="workerId">the worker id of message sender</param>
        public void CollectMessageData(MessagePackage message, int workerId)
        {
            if (workerId < 0 || workerId >= _maxWorkerCount)
            {
                throw new InvalidOperationException("message's FromId is out of range");
            }

            if (message.SendTimeTicks <= 0 || message.ReceiveTimeTicks <= 0)
            {
                // ignore the messages that have invalid timestamps.
                return;
            }

            Interlocked.Increment(ref _messageCount);

            long ticks = message.ReceiveTimeTicks - message.SendTimeTicks;
            if (ticks < 0)
            {
                Interlocked.Increment(ref _negativeTimeMessageCount);
            }

            _counterByWorker[workerId].AddSample(ticks);
        }

        public string GetStatisticInformation()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat(
                "Type {0} : Total# {1}, Negative Time# {2}",
                (MessageType)_type,
                _messageCount,
                _negativeTimeMessageCount);

            builder.AppendLine();
            for (int i = 0; i < _counterByWorker.Length; ++i)
            {
                if (_counterByWorker[i].Count > 0)
                {
                    MessageTimeCounter counter = _counterByWorker[i];

                    builder.AppendFormat(
                        "{0,-4}: {1,8}, {2:F8}, {3:F8}, {4:F8}",
                        i,
                        counter.Count,
                        ConvertTicksToMillisecond(counter.GetTotalTicks()) / counter.Count,
                        ConvertTicksToMillisecond(counter.GetMaxTicks()),
                        ConvertTicksToMillisecond(counter.GetMinTicks()));

                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        private static double ConvertTicksToMillisecond(long ticks)
        {
            TimeSpan span = new TimeSpan(ticks);
            return span.TotalMilliseconds;
        }
    }
}
