namespace ParallelFastRank
{
    using System;

    public interface ICommunicationChannel : IDisposable
    {
        /// <summary>
        /// Gets the status of the communicator
        /// </summary>
        string Status { get; }

        /// <summary>
        /// The action that need help in other threads if they can.
        /// </summary>
        Action ActionNeedHelp { get; }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">the message to be sent</param>
        void Send(MessagePackage message);
    }
}
