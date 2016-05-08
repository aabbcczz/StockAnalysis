namespace ParallelFastRank
{
    using System;
    using System.Linq;

    using FastRank;

    /// <summary>
    /// Worker machine information
    /// </summary>
    public sealed class WorkerMachineInfo
    {
        private const int APNameLength = 15;

        #region Properties

        public string Name { get; set; }

        public int Port { get; set; }

        public int Id { get; set; }

        /// <summary>
        /// Gets the full name. (Name + Port)
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName
        {
            get
            {
                return this.Name + ":" + this.Port;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this worker is an AP machine.
        /// </summary>
        /// <value>
        /// <c>true</c> if this worker is AP machine; otherwise, <c>false</c>.
        /// </value>
        public bool IsAPMachine
        {
            get
            {
                // AP machine name is like CH1SCH050011108.
                return this.Name != null && this.Name.Length == APNameLength && this.Name.All(Char.IsLetterOrDigit);
            }
        }

        /// <summary>
        /// Gets the name of the pod. Only valid if IsAPMachine is true.
        /// </summary>
        /// <value>
        /// The name of the pod. null if not AP machine.
        /// </value>
        public string PodName
        {
            get
            {
                return this.IsAPMachine ? this.Name.Substring(0, APNameLength - 2) : null;
            }
        }

        #endregion

        #region Ctors

        public WorkerMachineInfo(string name, int port, int id)
        {
            this.Name = name;
            this.Port = port;
            this.Id = id;
        }

        public WorkerMachineInfo(byte[] buffer, ref int position)
        {
            this.Name = buffer.ToString(ref position);
            this.Port = buffer.ToInt(ref position);
            this.Id = buffer.ToInt(ref position);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the size in bytes
        /// </summary>
        /// <returns>The size</returns>
        public int SizeInBytes()
        {
            return this.Name.SizeInBytes() + this.Port.SizeInBytes() + this.Id.SizeInBytes();
        }

        /// <summary>
        /// Serialize to bytes
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="position">The position.</param>
        public void ToByteArray(byte[] buffer, ref int position)
        {
            this.Name.ToByteArray(buffer, ref position);
            this.Port.ToByteArray(buffer, ref position);
            this.Id.ToByteArray(buffer, ref position);
        }

        public override string ToString()
        {
            return String.Format("[{0}]{1}", Id, FullName);
        }

        #endregion
    }
}
