namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FastRank;

    /// <summary>
    /// message for sync time between local machine and server
    /// </summary>
    internal class TimeSyncMessage : MessagePackage
    {
        public TimeSyncMessage()
            : base(MessageType.TimeSync)
        {
            ServerTicks = 0;
        }

        public TimeSyncMessage(int from, int to)
            : base(MessageType.TimeSync, from, to)
        {
            ServerTicks = 0;
        }

        /// <summary>
        /// Ticks in server.
        /// The extract meaning of tick depends on caller.
        /// </summary>
        public long ServerTicks { get; set; }

        public override int GetSerializedContentLength()
        {
            return base.GetSerializedContentLength() + sizeof(long);
        }

        public override void Serialize(byte[] buffer, ref int offset)
        {
            base.Serialize(buffer, ref offset);

            ServerTicks.ToByteArray(buffer, ref offset);
        }

        public override void Deserialize(byte[] buffer, int length, ref int offset)
        {
            base.Deserialize(buffer, length, ref offset);

            ServerTicks = buffer.ToLong(ref offset);
        }
    }
}
