namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    /// <summary>
    /// This class is used for collecting message latency.
    /// The synchronization of time is taken care by other classes
    /// </summary>
    public sealed class MessageTimeCollector
    {
        /// <summary>
        /// the default global collector instance.
        /// </summary>
        private static MessageTimeCollector defaultCollector = null;

        /// <summary>
        /// The array of message time collector for each message type.
        /// </summary>
        private SingleTypeMessageTimeCollector[] _collectorsByType;

        /// <summary>
        /// Construct an instance of message time collector
        /// </summary>
        /// <param name="maxWorkerCount">maximum number of workers that could send message</param>
        public MessageTimeCollector(int maxWorkerCount)
        {
            _collectorsByType = new SingleTypeMessageTimeCollector[(int)MessageType.TypeCount];
            for (int i = 0; i < (int)MessageType.TypeCount; ++i)
            {
                _collectorsByType[i] = new SingleTypeMessageTimeCollector((MessageType)i, maxWorkerCount);
            }
        }
        
        /// <summary>
        /// Initialize the default/global message time collector
        /// </summary>
        /// <param name="maxWorkerCount">maximum number of workers that could send message</param>
        public static void Initialize(int maxWorkerCount)
        {
            MessageTimeCollector.defaultCollector = new MessageTimeCollector(maxWorkerCount);
        }

        /// <summary>
        /// Collect message data by using the global message time collector.
        /// </summary>
        /// <param name="message">message with data to be collected</param>
        public static void CollectMessageDataGlobally(MessagePackage message)
        {
            if (MessageTimeCollector.defaultCollector != null)
            {
                MessageTimeCollector.defaultCollector.CollectMessageData(message);
            }
        }

        /// <summary>
        /// Get the statistics of global message time collector
        /// </summary>
        /// <returns>the status information</returns>
        public static string GetStatisticInformationGlobally()
        {
            return MessageTimeCollector.defaultCollector == null
                ? string.Empty
                : MessageTimeCollector.defaultCollector.GetStatisticInformation();
        }

        /// <summary>
        /// Collect message data
        /// </summary>
        /// <param name="message">message with data to be collected</param>
        public void CollectMessageData(MessagePackage message)
        {
            _collectorsByType[(int)message.Type].CollectMessageData(message, message.FromId);
        }

        public string GetStatisticInformation()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < (int)MessageType.TypeCount; ++i)
            {
                if (_collectorsByType[i].MessageCount == 0)
                {
                    continue;
                }

                builder.AppendLine(_collectorsByType[i].GetStatisticInformation());
            }

            return builder.ToString();
        }
    }
}
