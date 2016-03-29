namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using FastRank;

    [Serializable]
    public class MessagePackage
    {
        private const uint MagicNumber = 0x5047534D; // "MSGP"
        private const int CommonHeaderSize = 12; // magic number + type + size fields;

        private static Dictionary<MessageType, Type> registeredMessagePackageTypes 
            = new Dictionary<MessageType, Type>();

        private static int serialNumber = 0;

        private uint _messageSerialNumber = MessagePackage.NextSerialNumber;

        #region constructors

        static MessagePackage()
        {
            // we can only handle loaded assemblies. if a sub-class of MessagePackage is defined in
            // an delay loaded assembly, our program will have problem.
            AppDomain domain = AppDomain.CurrentDomain;
            Assembly[] assemblies = domain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = null;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // an ReflectionTypeLoadException may be thrown out due to some non-existing assembly on local machine, for example an assembly
                    // may have dependencies on some other assemblies which haven't been loaded yet. 
                    // If the assembly hasn't been loaded when the constructor is called, but it has some kinds of message types, the program may
                    // run into problem.
                    continue;
                }

                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(MessagePackage)))
                    {
                        MessagePackage obj = (MessagePackage)Activator.CreateInstance(type);
                        if (obj.Type != MessageType.None)
                        {
                            RegisterSelfType(obj.Type, type);
                        }
                    }
                }
            }
        }

        public MessagePackage()
        {
            SupportMultipleBufferSerialization = false;
            Type = MessageType.None;
            UserDefinedObject = null;
            SendTimeTicks = 0;
            ReceiveTimeTicks = 0;
        }

        public MessagePackage(MessageType type)
            : this()
        {
            Type = type;
        }

        public MessagePackage(MessageType type, int from, int to)
            : this(type)
        {
            FromId = from;
            ToId = to;
        }

        #endregion
        /// <summary>
        /// user defined object, it will not be serialized/deserialized. 
        /// </summary>
        public object UserDefinedObject { get; set; }

        /// <summary>
        /// Ticks of the time when the message is sent. this field will be serialized.
        /// </summary>
        public long SendTimeTicks { get; set; }

        /// <summary>
        /// Ticks of the time when the message is received. this field will not be serialized.
        /// </summary>
        public long ReceiveTimeTicks { get; set; }

        public int FromId { get; set; }

        public int ToId { get; set; }

        public MessageType Type { get; set; }

        public uint MessageSerialNumber
        {
            get { return _messageSerialNumber; }
        }

        public uint ApplicationSerialNumber { get; set; }

        protected static uint NextSerialNumber
        {
            get { return (uint)Interlocked.Increment(ref serialNumber); }
        }

        // whether this class support calling IList<ArraySegment<byte>> Serialize() function, default is false.
        protected bool SupportMultipleBufferSerialization { get; set; }

        public static ArraySegment<byte> AllocateSerializationMemory(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

#if NO_BUFFER_MANAGER
            return new ArraySegment<byte>(new byte[size], 0, size);
#else
            return BufferManager.Instance.TakeBuffer(size);
#endif
        }

        public static void FreeSerializationMemory(IList<ArraySegment<byte>> buffers)
        {
            if (buffers == null)
            {
                throw new ArgumentNullException("buffers");
            }

#if NO_BUFFER_MANAGER
#else
            BufferManager.Instance.ReturnBuffers(buffers);
#endif
        }

        public static int GetSerializedContentLength(MessagePackage message)
        {
            return message.GetSerializedContentLength() + MessagePackage.CommonHeaderSize;
        }

        public static void SeralizeMessage(MessagePackage message, byte[] buffer, ref int offset)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            using (FastRank.Timer.Time(TimerEvent.MessageSerialize))
            {
                int size = 0; // size of all serialized message, include header part.
                int beginOffset = offset; // the start offset

                // save common message header;
                MagicNumber.ToByteArray(buffer, ref offset);
                ((int)message.Type).ToByteArray(buffer, ref offset);

                int offsetOfSizeField = offset; // remember the offset to store "size" field;
                size.ToByteArray(buffer, ref offset);

                // serialize message
                message.Serialize(buffer, ref offset);

                // rewrite size field
                size = offset - beginOffset;
                size.ToByteArray(buffer, ref offsetOfSizeField);
            }
        }

        public static IList<ArraySegment<byte>> SeralizeMessage(MessagePackage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            using (FastRank.Timer.Time(TimerEvent.MessageSerialize))
            {
                List<ArraySegment<byte>> resultList = new List<ArraySegment<byte>>();

                if (message.SupportMultipleBufferSerialization)
                {
                    ArraySegment<byte> buffer = MessagePackage.AllocateSerializationMemory(MessagePackage.CommonHeaderSize);

                    int offset = buffer.Offset;

                    // save common message header;
                    MagicNumber.ToByteArray(buffer.Array, ref offset);
                    ((int)message.Type).ToByteArray(buffer.Array, ref offset);
                    int size = MessagePackage.GetSerializedContentLength(message);
                    size.ToByteArray(buffer.Array, ref offset);

                    System.Diagnostics.Debug.Assert(
                        offset == buffer.Offset + MessagePackage.CommonHeaderSize,
                        "serialize message header failed");

                    resultList.Add(buffer);
                    resultList.AddRange(message.Serialize());
                }
                else
                {
                    ArraySegment<byte> buffer = MessagePackage.AllocateSerializationMemory(MessagePackage.GetSerializedContentLength(message));
                    int offset = buffer.Offset;
                    MessagePackage.SeralizeMessage(message, buffer.Array, ref offset);

                    System.Diagnostics.Debug.Assert(
                        offset == buffer.Offset + buffer.Count,
                        "Serialize message failed");

                    resultList.Add(buffer);
                }

                return resultList;
            }
        }

        public static MessagePackage DeserializeMessage(byte[] buffer, int length, ref int offset)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            using (FastRank.Timer.Time(TimerEvent.MessageDeserialize))
            {
                if (length - offset < MessagePackage.CommonHeaderSize)
                {
                    throw new InvalidMessageException("buffer length is not enough");
                }

                int beginOffset = offset; // reserve beginning offset

                uint magicNumber = buffer.ToUInt(ref offset);

                if (magicNumber != MessagePackage.MagicNumber)
                {
                    throw new InvalidMessageException("not a valid message package. Magic number is incorrect");
                }

                MessageType type = (MessageType)buffer.ToInt(ref offset);
                int size = buffer.ToInt(ref offset);

                MessagePackage message = MessagePackage.CreateMessagePackage(type);
                message.Deserialize(buffer, length, ref offset);

                if (type != message.Type)
                {
                    throw new InvalidMessageException(string.Format("type {0} in header is different with actual message type {1}", type, message.Type));
                }

                if (offset - beginOffset != size)
                {
                    throw new InvalidMessageException("buffer might be corrupted. the size stored in header is not consistent with real size");
                }

                return message;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} => {1} {2} {3:X8} {4:X8}",
                FromId,
                ToId,
                Type,
                MessageSerialNumber,
                ApplicationSerialNumber);
        }

        public virtual int GetSerializedContentLength()
        {
            return (5 * sizeof(int)) + sizeof(long); // 5 integer fields + 1 long field
        }

        public virtual void Serialize(byte[] buffer, ref int offset)
        {
            _messageSerialNumber.ToByteArray(buffer, ref offset);
            ApplicationSerialNumber.ToByteArray(buffer, ref offset);
            FromId.ToByteArray(buffer, ref offset);
            ToId.ToByteArray(buffer, ref offset);
            SendTimeTicks.ToByteArray(buffer, ref offset);
            ((int)Type).ToByteArray(buffer, ref offset);
        }

        public virtual void Deserialize(byte[] buffer, int length, ref int offset)
        {
            _messageSerialNumber = buffer.ToUInt(ref offset);
            ApplicationSerialNumber = buffer.ToUInt(ref offset);
            FromId = buffer.ToInt(ref offset);
            ToId = buffer.ToInt(ref offset);
            SendTimeTicks = buffer.ToLong(ref offset);
            Type = (MessageType)buffer.ToInt(ref offset);
        }

        public virtual IList<ArraySegment<byte>> Serialize()
        {
            throw new NotImplementedException();
        }

        protected static void RegisterSelfType(MessageType type, Type classType)
        {
            if (!classType.IsSubclassOf(typeof(MessagePackage)))
            {
                throw new InvalidOperationException("only sub-class of MessagePackage can be registered");
            }

            if (registeredMessagePackageTypes.ContainsKey(type))
            {
                registeredMessagePackageTypes[type] = classType;
            }
            else
            {
                registeredMessagePackageTypes.Add(type, classType);
            }
        }

        private static MessagePackage CreateMessagePackage(MessageType type)
        {
            if (!registeredMessagePackageTypes.ContainsKey(type))
            {
                throw new InvalidOperationException(string.Format("no MessagePackage class is registered for type {0}", type));
            }

            Type classType = registeredMessagePackageTypes[type];

            System.Diagnostics.Debug.Assert(
                classType.IsSubclassOf(typeof(MessagePackage)),
                "registered message type is not derived from MessagePackage");

            MessagePackage message = (MessagePackage)Activator.CreateInstance(classType);

            return message;
        }
    }
}
