namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FastRank;

    internal sealed class WorkerMachineListMessage : MessagePackage
    {
        public WorkerMachineListMessage()
            : base(MessageType.WorkerMachineList)
        {
            MachineList = null;
        }

        public WorkerMachineListMessage(WorkerMachineInfo[] machines)
            : this()
        {
            MachineList = machines;
        }

        public WorkerMachineInfo[] MachineList { get; private set; }

        public override int GetSerializedContentLength()
        {
            return base.GetSerializedContentLength() + sizeof(int) + MachineList.Sum(x => x.SizeInBytes());
        }

        public override void Serialize(byte[] buffer, ref int offset)
        {
            base.Serialize(buffer, ref offset);

            int count = MachineList.Length;
            count.ToByteArray(buffer, ref offset);

            for (int i = 0; i < MachineList.Length; i++)
            {
                MachineList[i].ToByteArray(buffer, ref offset);
            }
        }

        public override void Deserialize(byte[] buffer, int length, ref int offset)
        {
            base.Deserialize(buffer, length, ref offset);

            int workerCount = buffer.ToInt(ref offset);

            MachineList = new WorkerMachineInfo[workerCount];
            for (int i = 0; i < workerCount; i++)
            {
                MachineList[i] = new WorkerMachineInfo(buffer, ref offset);
            }
        }
    }
}
