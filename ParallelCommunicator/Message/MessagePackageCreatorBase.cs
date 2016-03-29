namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class MessagePackageCreatorBase
    {
        // master aggregation message
        public static void SetMasterAggregationMessage(int from, MessagePackage message)
        {
            message.FromId = from;
            message.ToId = CommunicationEndPoint.MasterWorkerId;
        }

        // master broadcast message
        public static void SetMasterBroadcastMessage(MessagePackage message)
        {
            message.FromId = CommunicationEndPoint.MasterWorkerId;
            message.ToId = CommunicationEndPoint.ToAllWorkerId;
        }
    }
}
