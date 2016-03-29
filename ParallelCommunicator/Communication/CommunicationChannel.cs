namespace ParallelFastRank
{
    using System;

    public static class CommunicationChannel
    {
        public static ICommunicationChannel Channel { get; private set; }

        public static string Status
        {
            get
            {
                if (Channel != null)
                {
                    return Channel.Status;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static void SetChannel(ICommunicationChannel channelImplementation)
        {
            // Set only once
            if (Channel == null)
            {
                Channel = channelImplementation;
            }
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="msg">message to be sent</param>
        public static void Send(MessagePackage msg)
        {
            if (Channel == null)
            {
                throw new InvalidOperationException("Underlying communication channel is not initialized");
            }

            Channel.Send(msg);
        }

        public static void HelpChannel()
        {
            if (Channel != null)
            {
                if (Channel.ActionNeedHelp != null)
                {
                    Channel.ActionNeedHelp();
                }
            }
        }

        public static void Dispose()
        {
            if (Channel != null)
            {
                Channel.Dispose();
                Channel = null;
            }
        }
    }
}
