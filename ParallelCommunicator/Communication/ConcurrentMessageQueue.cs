namespace ParallelFastRank
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public sealed class ConcurrentMessageQueue
    {
        private BlockingCollection<MessagePackage>[] _messageQueues; // each type has a queue
        private int _queueSize; // the count of messages in all queues;

        public ConcurrentMessageQueue()
        {
            _messageQueues = new BlockingCollection<MessagePackage>[(int)MessageType.TypeCount];
            for (int i = 0; i < (int)MessageType.TypeCount; i++)
            {
                _messageQueues[i] = new BlockingCollection<MessagePackage>(new ConcurrentQueue<MessagePackage>());
            }

            _queueSize = 0;
        }

        public int Size
        {
            get { return Interlocked.CompareExchange(ref _queueSize, 0, 0); }
        }

        public void PutMessage(MessagePackage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            IncreaseSize();

            _messageQueues[(int)message.Type].Add(message);
        }

        public MessagePackage GetMessage()
        {
            MessagePackage message;

            BlockingCollection<MessagePackage>.TakeFromAny(_messageQueues, out message);

            DecreaseSize();

            return message;
        }

        public IEnumerable<MessagePackage> GetAllMessages(MessageType type, int timeout)
        {
            int leftTime = timeout;
            MessagePackage message;

            while (leftTime > 0)
            {
                // try to get all existing messages until the queue is empty.
                while (_messageQueues[(int)type].TryTake(out message))
                {
                    yield return message;
                }

                DateTime startTime = DateTime.Now;

                // wait for new message
                if (!_messageQueues[(int)type].TryTake(out message, leftTime))
                {
                    break;
                }

                yield return message;

                DateTime endTime = DateTime.Now;

                leftTime -= (int)(endTime - startTime).TotalMilliseconds;
            }
        }

        public MessagePackage GetMessage(MessageType messageType)
        {
            return GetMessage(messageType, CancellationToken.None);
        }

        public MessagePackage GetMessage(MessageType messageType, CancellationToken token)
        {
            MessagePackage message = _messageQueues[(int)messageType].Take(token);

            DecreaseSize();

            return message;
        }

        public MessagePackage GetMessage(MessageType messageType, int timeout)
        {
            MessagePackage message;

            if (!_messageQueues[(int)messageType].TryTake(out message, timeout))
            {
                throw new TimeoutException(
                    string.Format(
                        "no Message {1} Received in {0:f4} min",
                        (double)timeout / 1000.0 / 60.0, 
                        messageType));
            }

            DecreaseSize();

            return message;
        }

        public MessagePackage GetMessage(MessageType[] messageTypes)
        {
            return GetMessage(messageTypes, CancellationToken.None);
        }

        public MessagePackage GetMessage(MessageType[] messageTypes, CancellationToken token)
        {
            MessagePackage message;

            BlockingCollection<MessagePackage>[] collections 
                = messageTypes.Select(type => _messageQueues[(int)type]).ToArray();

            BlockingCollection<MessagePackage>.TakeFromAny(collections, out message, token);

            DecreaseSize();

            return message;
        }

        public MessagePackage GetMessage(MessageType[] messageTypes, int timeout)
        {
            if (messageTypes == null)
            {
                throw new ArgumentNullException("messageTypes");
            }

            MessagePackage message;

            BlockingCollection<MessagePackage>[] collections
                = messageTypes.Select(type => _messageQueues[(int)type]).ToArray();

            if (-1 == BlockingCollection<MessagePackage>.TryTakeFromAny(collections, out message, timeout))
            {
                StringBuilder sb = new StringBuilder();
                foreach (MessageType type in messageTypes)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append('/');
                    }

                    sb.Append(type.ToString());
                }

                throw new TimeoutException(
                    string.Format(
                        "no Message {1} Received in {0:f4} min",
                        (double)timeout / 1000.0 / 60.0, 
                        sb.ToString()));
            }

            DecreaseSize();

            return message;
        }

        public bool TryGetMessage(MessageType messageType, out MessagePackage message)
        {
            return TryGetMessage(messageType, out message, 0);
        }

        public bool TryGetMessage(MessageType messageType, out MessagePackage message, int timeout)
        {
            if (_messageQueues[(int)messageType].TryTake(out message, timeout))
            {
                DecreaseSize();

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetMessage(MessageType[] messageTypes, out MessagePackage message)
        {
            return TryGetMessage(messageTypes, out message, 0);
        }

        public bool TryGetMessage(MessageType[] messageTypes, out MessagePackage message, int timeout)
        {
            BlockingCollection<MessagePackage>[] collections
                = messageTypes.Select(type => _messageQueues[(int)type]).ToArray();

            if (-1 != BlockingCollection<MessagePackage>.TryTakeFromAny(collections, out message, timeout))
            {
                DecreaseSize();
                return true;
            }
            else
            {
                return false;
            }
        }

        private int IncreaseSize()
        {
            return Interlocked.Increment(ref _queueSize);
        }

        private int DecreaseSize()
        {
            return Interlocked.Decrement(ref _queueSize);
        }
    }
}
