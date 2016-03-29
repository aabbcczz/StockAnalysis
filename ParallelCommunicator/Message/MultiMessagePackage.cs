namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FastRank;

    public class MultiMessagePackage : MessagePackage
    {
        private List<MessageType> _acceptableTypes = null;

        private List<MessagePackage> _messages = new List<MessagePackage>();

        #region constructors

        public MultiMessagePackage()
            : base()
        {
            SupportMultipleBufferSerialization = true;
        }

        public MultiMessagePackage(MessageType type)
            : base(type)
        {
            SupportMultipleBufferSerialization = true;
        }

        public MultiMessagePackage(MessageType type, IEnumerable<MessageType> acceptableTypes)
            : this(type)
        {
            SupportMultipleBufferSerialization = true;

            if (acceptableTypes != null)
            {
                acceptableTypes = new List<MessageType>(acceptableTypes);
            }
        }

        #endregion

        public IEnumerable<MessagePackage> Messages
        {
            get { return _messages; }
        }

        public int Count
        {
            get { return _messages.Count; }
        }

        // must be call in single thread environment.
        public virtual void Reset()
        {
            _messages.Clear();
            ApplicationSerialNumber = 0;
        }

        public virtual void AddMessage(MessagePackage msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException("msg");
            }

            if (_acceptableTypes != null)
            {
                if (!_acceptableTypes.Exists(x => x == msg.Type))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Message package with type {0} is not allowed to be added in to this collection",
                            msg.Type));
                }
            }

            _messages.Add(msg);
        }

        public override int GetSerializedContentLength()
        {
            int length = base.GetSerializedContentLength() +
                            sizeof(int) +  // _messageCount
                            _messages.Sum(x => MessagePackage.GetSerializedContentLength(x));

            return length;
        }

        // not thread safe
        public override void Serialize(byte[] buffer, ref int offset)
        {
            base.Serialize(buffer, ref offset);

            _messages.Count.ToByteArray(buffer, ref offset);

            foreach (var msg in _messages)
            {
                MessagePackage.SeralizeMessage(msg, buffer, ref offset);
            }
        }

        public override void Deserialize(byte[] buffer, int length, ref int offset)
        {
            _messages.Clear();

            base.Deserialize(buffer, length, ref offset);

            int messageCount = buffer.ToInt(ref offset);

            for (int i = 0; i < messageCount; i++)
            {
                _messages.Add(MessagePackage.DeserializeMessage(buffer, length, ref offset));
            }
        }

        public override IList<ArraySegment<byte>> Serialize()
        {
            List<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>();

            ArraySegment<byte> baseBuffer = MessagePackage.AllocateSerializationMemory(base.GetSerializedContentLength() + sizeof(int));

            int offset = baseBuffer.Offset;
            base.Serialize(baseBuffer.Array, ref offset);
            _messages.Count.ToByteArray(baseBuffer.Array, ref offset);

            System.Diagnostics.Debug.Assert(
                offset == baseBuffer.Offset + base.GetSerializedContentLength() + sizeof(int),
                "serization base class failed");

            buffers.Add(baseBuffer);

            foreach (var msg in _messages)
            {
                buffers.AddRange(MessagePackage.SeralizeMessage(msg));
            }

            return buffers;
        }
    }
}
