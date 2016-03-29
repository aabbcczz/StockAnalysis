namespace ParallelFastRank
{
    using System;
    using System.Threading;
    using FastRank;

    /// <summary>
    /// Base class of network message communicator for parallel training.
    /// </summary>
    public abstract class MessageCommunicatorBase : IDisposable
    {
        private ConcurrentMessageQueue _receivedMessageQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCommunicatorBase"/> class.
        /// </summary>
        /// <param name="totalWorkerCount">The worker count.</param>
        /// <param name="workerId">The worker's id (starting from zero).</param>
        /// <param name="name">The name of the communicator.</param>
        protected MessageCommunicatorBase(int totalWorkerCount, int workerId, string name)
        {
            TotalWorkerCount = totalWorkerCount;
            WorkerId = workerId;
            Name = name;

            _receivedMessageQueue = new ConcurrentMessageQueue();
        }

        /// <summary>
        /// Gets the name of the communicator.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the worker's worker id which starts from zero.
        /// </summary>
        public int WorkerId { get; private set; }

        /// <summary>
        /// Gets the total worker count used for the parallel training job.
        /// </summary>
        public int TotalWorkerCount { get; private set; }

        /// <summary>
        /// The abstract method for sending a message package.
        /// </summary>
        /// <param name="msg">The instance of the <see cref="MessagePackage"/> class to send.</param>
        public abstract void Send(MessagePackage msg);

        /// <summary>
        /// The abstract method for Recieving a message package.
        /// </summary>
        /// <param name="msg">The received instance of the <see cref="MessagePackage"/> class.</param>
        public abstract void Receive(MessagePackage msg);

        /// <summary>
        /// The method for sending a message package. It'll use the overrided <see cref="Send"/> method to send the message package.
        /// </summary>
        /// <param name="message">The instance of the <see cref="MessagePackage"/> class to send.</param>
        public void SendMessage(MessagePackage message)
        {
            Send(message);
        }

        /// <summary>
        /// The method for retrieving a specific type <see cref="MessageType"/> of message package from the received message queue of the communicator with timeout.
        /// If the communicator cannot receive the message in a specific time, an <see cref="TimeoutException"/> would be raised.
        /// If the required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="type">The type of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="timeout">The number of milliseconds to wait before an <see cref="TimeoutException"/> is raised.</param>
        /// <returns>The retrieved message.</returns>
        public MessagePackage ReceiveMessage(MessageType type, int timeout)
        {
            return _receivedMessageQueue.GetMessage(type, timeout);
        }

        /// <summary>
        /// The method for retrieving a specific type <see cref="MessageType"/> of message package from the received message queue of the communicator.
        /// The method will wait forever untill the required message is received.
        /// If the required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="type">The type of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <returns>The retrieved message.</returns>
        public MessagePackage ReceiveMessage(MessageType type)
        {
            return _receivedMessageQueue.GetMessage(type);
        }

        /// <summary>
        /// The method for retrieving a specific type <see cref="MessageType"/> of message package from the received message queue of the communicator with canllelation token.
        /// The method will wait forever unless the given <see cref="CancellationToken"/> is canceled.
        /// If the required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="type">The type of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="token">The instance of the <see cref="CancellationToken"/> class.</param>
        /// <returns>The retrieved message.</returns>
        public MessagePackage ReceiveMessage(MessageType type, CancellationToken token)
        {
            return _receivedMessageQueue.GetMessage(type, token);
        }

        /// <summary>
        /// The method for retrieving any of specific types <see cref="MessageType"/> of message package from the received message queue of the communicator with timeout.
        /// The method will wait forever untill the required message is received.
        /// If any required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="types">The types of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="timeout">The number of milliseconds to wait before an <see cref="TimeoutException"/> is raised.</param>
        /// <returns>The retrieved message.</returns>
        public MessagePackage ReceiveMessage(MessageType[] types, int timeout)
        {
            return _receivedMessageQueue.GetMessage(types, timeout);
        }

        /// <summary>
        /// The method for retrieving any of specific types <see cref="MessageType"/> of message package from the received message queue of the communicator.
        /// If the communicator cannot receive the message in a specific time, an <see cref="TimeoutException"/> would be raised.
        /// If any required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="types">The types of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <returns>The retrieved message.</returns>
        public MessagePackage ReceiveMessage(MessageType[] types)
        {
            return _receivedMessageQueue.GetMessage(types);
        }

        /// <summary>
        /// The method for retrieving any of specific types <see cref="MessageType"/> of message package from the received message queue of the communicator with canllelation token.
        /// The method will wait forever unless the given <see cref="CancellationToken"/> is canceled.
        /// If the required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="types">The types of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="token">The instance of the <see cref="CancellationToken"/> class.</param>
        /// <returns>The retrieved message.</returns>
        public MessagePackage ReceiveMessage(MessageType[] types, CancellationToken token)
        {
            return _receivedMessageQueue.GetMessage(types, token);
        }

        /// <summary>
        /// The method for trying to retrieve a specific type <see cref="MessageType"/> of message package from the received message queue of the communicator with timeout.
        /// If the required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="type">The type of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="message">[out] the received message.</param>
        /// <param name="timeout">The number of milliseconds to wait before an <see cref="TimeoutException"/> is raised.</param>
        /// <returns>false if the communicator cannot receive the message in a specific time, true otherwise.</returns>
        public bool TryReceiveMessage(MessageType type, out MessagePackage message, int timeout)
        {
            return _receivedMessageQueue.TryGetMessage(type, out message, timeout);
        }

        /// <summary>
        /// The method for trying to retrieve a specific type <see cref="MessageType"/> of message package from the received message queue of the communicator.
        /// The method will wait forever untill the required message is received.
        /// If the required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="type">The type of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="message">[out] the received message.</param>
        /// <returns>false if the communicator cannot receive the message in a specific time, true otherwise.</returns>
        public bool TryReceiveMessage(MessageType type, out MessagePackage message)
        {
            return _receivedMessageQueue.TryGetMessage(type, out message);
        }

        /// <summary>
        /// The method for trying to retrieve any of specific types <see cref="MessageType"/> of message package from the received message queue of the communicator with timeout.
        /// If any required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="types">The types of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="message">[out] the received message.</param>
        /// <param name="timeout">The number of milliseconds to wait before it returns.</param>
        /// <returns>false if the communicator cannot receive the message in a specific time, true otherwise.</returns>
        public bool TryReceiveMessage(MessageType[] types, out MessagePackage message, int timeout)
        {
            return _receivedMessageQueue.TryGetMessage(types, out message, timeout);
        }

        /// <summary>
        /// The method for retrieving any of specific types <see cref="MessageType"/> of message package from the received message queue of the communicator.
        /// The method will wait forever untill the required message is received.
        /// If any required message is successfully received, the message would be removed from the received message queue of the communicator.
        /// </summary>
        /// <param name="types">The types of the message to receive. <see cref="MessageType"/> for all supported message types.</param>
        /// <param name="message">[out] the received message.</param>
        /// <returns>false if the communicator cannot receive the message in a specific time, true otherwise.</returns>
        public bool TryReceiveMessage(MessageType[] types, out MessagePackage message)
        {
            return _receivedMessageQueue.TryGetMessage(types, out message);
        }

        /// <summary>
        /// Puts a message package to the received message queue of the communicator.
        /// </summary>
        /// <param name="message">The message to put.</param>
        public void PutMessageToReceiveQueue(MessagePackage message)
        {
            _receivedMessageQueue.PutMessage(message);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            StaticRuntimeContext.Stdout.WriteLine(MessageCommunicationStatistics.GetGlobalStatistics());
        }

        #endregion
    }
}
