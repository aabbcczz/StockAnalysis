namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal sealed class WorkerMachineInfoMessage : MessagePackage
    {
        public WorkerMachineInfoMessage()
            : base(MessageType.WorkerMachineInfo)
        {
            MachineInfo = null;
        }

        public WorkerMachineInfoMessage(WorkerMachineInfo info)
            : this()
        {
            MachineInfo = info;
        }

        public WorkerMachineInfo MachineInfo { get; private set; }

        public override int GetSerializedContentLength()
        {
            return base.GetSerializedContentLength() + MachineInfo.SizeInBytes();
        }

        public override void Serialize(byte[] buffer, ref int offset)
        {
            base.Serialize(buffer, ref offset);

            MachineInfo.ToByteArray(buffer, ref offset);
        }

        public override void Deserialize(byte[] buffer, int length, ref int offset)
        {
            base.Deserialize(buffer, length, ref offset);

            MachineInfo = new WorkerMachineInfo(buffer, ref offset);
        }
    }
}
