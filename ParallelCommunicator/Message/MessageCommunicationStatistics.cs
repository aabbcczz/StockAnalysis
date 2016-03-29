namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class MessageCommunicationStatistics
    {
        static MessageCommunicationStatistics()
        {
            RoundSendingStatus = new MessageStatistics();
            RoundReceiveStatus = new MessageStatistics();
            GlobalSendingStatus = new MessageStatistics();
            GlobalReceiveStatus = new MessageStatistics();
        }

        public static MessageStatistics RoundSendingStatus { get; private set; }

        public static MessageStatistics RoundReceiveStatus { get; private set; }

        public static MessageStatistics GlobalSendingStatus { get; private set; }

        public static MessageStatistics GlobalReceiveStatus { get; private set; }

        public static string GetRoundStatistics()
        {
            StringBuilder statistics = new StringBuilder();

            statistics.AppendLine("Sender Round Status:");
            statistics.AppendLine(RoundSendingStatus.GetStatistics());
            RoundSendingStatus.Reset();

            statistics.AppendLine("Receiver Round Status:");
            statistics.AppendLine(RoundReceiveStatus.GetStatistics());
            RoundReceiveStatus.Reset();

            return statistics.ToString();
        }

        public static string GetGlobalStatistics()
        {
            StringBuilder statistics = new StringBuilder();

            statistics.AppendLine("Sender Global Status:");
            statistics.AppendLine(GlobalSendingStatus.GetStatistics());
            statistics.AppendLine("Receiver Global Status:");
            statistics.AppendLine(GlobalReceiveStatus.GetStatistics());

            return statistics.ToString();
        }

        public static void ResetRoundStatus()
        {
            RoundSendingStatus.Reset();
            RoundReceiveStatus.Reset();
        }
    }
}
