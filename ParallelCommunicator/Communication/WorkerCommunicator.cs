namespace ParallelFastRank
{
    using System;
    using System.Linq;
    using System.Threading;

    using FastRank;

    // return true means the message has been processed and should not be put in queue again
    public delegate bool ReceivedMessageDelegate(MessagePackage message);

    public sealed class WorkerCommunicator : MessageCommunicatorBase
    {
        public const uint InvalidApplicationSerialNumber = 0xFFFFFFFF;

        private ReceivedMessageDelegate _onReceivedMessage = null;

        private ReaderWriterLockSlim _onReceivedMessageLocker = new ReaderWriterLockSlim();

        #region Properties

        /// <summary>
        /// Gets a value indicating whether hierarchical communication is enabled
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable hierarchical communication]; otherwise, <c>false</c>.
        /// </value>
        public bool IsHierarchicalCommunicationEnabled { get; private set; }

        /// <summary>
        /// Gets the worker tree.
        /// </summary>
        /// <value>
        /// The worker tree.
        /// </value>
        public MachineTree WorkerTree { get; private set; }

        /// <summary>
        /// Gets the parent worker id.
        /// </summary>
        /// <value>
        /// The parent worker id.
        /// </value>
        public int ParentWorkerId { get; private set; }

        /// <summary>
        /// Gets the child worker ids.
        /// </summary>
        /// <value>
        /// The child worker ids.
        /// </value>
        public int[] ChildWorkerIds { get; private set; }

        /// <summary>
        /// Gets the child and self worker ids.
        /// </summary>
        /// <value>
        /// The child and self worker ids.
        /// </value>
        public int[] ChildAndSelfWorkerIds { get; private set; }

        /// <summary>
        /// Gets the worker count in the group this worker belongs to. 
        /// If worker group is not defined, all workers will be treated as in a single group,
        /// thus the value would be the total worker count in parallel enviroment.
        /// </summary>
        public int GroupWorkerCount
        {
            get { return (this.WorkerGroup == null || this.WorkerGroup.Length < 1) ? this.TotalWorkerCount : this.WorkerGroup.Length; }
        }
        
        /// <summary>
        /// Gets the worker id's of the group this worker belongs to.
        /// </summary>
        public int[] WorkerGroup { get; private set; }

        /// <summary>
        /// Gets the worker index in the worker group. 
        /// </summary>
        /// <remarks>
        /// For example, if the current worker id is 3, and it is in a worker group [2,3,6], the local worker id of current worker is 1.
        /// And if worker group is not defined, it is the same as worker id in parallel enviroment.
        /// </remarks>
        public int WorkerIndexInGroup { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is root node.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is root node; otherwise, <c>false</c>.
        /// </value>
        public bool IsRootNode
        {
            get
            {
                return this.ParentWorkerId == MachineTree.EmptyMachineId;
            }
        }

        #endregion
        
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerCommunicator"/> class.
        /// </summary>
        /// <param name="workerCount">The total worker count in current parallel enviroment.</param>
        /// <param name="workerId">The worker id.</param>
        /// <param name="enableHierarchicalCommunication">Whether hierarchical communication is enalbed.</param>
        /// <param name="workerGroup">The worker group this worker belongs to. Null means all workers are in a single group.</param>
        public WorkerCommunicator(int workerCount, int workerId, bool enableHierarchicalCommunication, int[] workerGroup) 
            : base(workerCount, workerId, string.Format("Worker {0}", workerId))
        {
            this.IsHierarchicalCommunicationEnabled = enableHierarchicalCommunication;
            this.ParentWorkerId = MachineTree.EmptyMachineId;
            this.WorkerGroup = workerGroup;
            this.WorkerIndexInGroup = workerGroup == null
                                     ? workerId
                                     : Array.FindIndex(workerGroup, x => x == workerId);

            if (workerGroup != null && (this.WorkerIndexInGroup < 0 || this.WorkerIndexInGroup >= workerGroup.Length))
            {
                throw new ArgumentOutOfRangeException("workerGroup", String.Format("Worker group is used, however the worker group donsn't contain the worker id {0}", workerId));
            }
        }

        /// <summary>
        /// Enables the hierarchical communication.
        /// </summary>
        public void EnableHierarchicalCommunication()
        {
            this.IsHierarchicalCommunicationEnabled = true;
        }

        /// <summary>
        /// Sets the worker tree.
        /// </summary>
        /// <param name="workerTree">The worker tree.</param>
        /// <exception cref="System.InvalidOperationException">Worker tree can't be set more than once</exception>
        public void SetWorkerTree(MachineTree workerTree)
        {
            if (!this.IsHierarchicalCommunicationEnabled)
            {
                return;
            }

            if (this.WorkerTree != null)
            {
                throw new InvalidOperationException("Worker tree can't be set more than once");
            }

            this.WorkerTree = workerTree;

            if (this.WorkerTree != null)
            {
                // dump tree for debugging
                StaticRuntimeContext.Stdout.WriteLine("Worker Tree:");
                StaticRuntimeContext.Stdout.WriteLine("{0}", workerTree);

                this.ParentWorkerId = this.WorkerTree.GetParent(this.WorkerId);
                this.ChildWorkerIds = this.WorkerTree.GetChildren(this.WorkerId).ToArray();
                this.ChildAndSelfWorkerIds = this.ChildWorkerIds.Union(Enumerable.Range(this.WorkerId, 1)).ToArray();
            }
        }

        #region Send/Receive Logic

        public override void Send(MessagePackage msg)
        {
#if MSG_DEBUG
            Console.WriteLine("[MSG] {0:X8} SI {1}", GetHashCode(), msg);
#endif
            CommunicationChannel.Send(msg);

#if MSG_DEBUG
            Console.WriteLine("[MSG] {0:X8} SX {1}", GetHashCode(), msg);
#endif
        }

        public void RegisterMessageReceiver(ReceivedMessageDelegate receiver)
        {
            _onReceivedMessageLocker.EnterWriteLock();
            try
            {
                _onReceivedMessage += receiver;
            }
            finally
            {
                _onReceivedMessageLocker.ExitWriteLock();
            }
        }

        public void UnregisterMessageReceiver(ReceivedMessageDelegate receiver)
        {
            _onReceivedMessageLocker.EnterWriteLock();
            try
            {
                _onReceivedMessage -= receiver;
            }
            finally
            {
                _onReceivedMessageLocker.ExitWriteLock();
            }
        }

        public override void Receive(MessagePackage msg)
        {
#if MSG_DEBUG
            Console.WriteLine("[MSG] {0:X8} RX {1}", GetHashCode(), msg);
#endif
            _onReceivedMessageLocker.EnterReadLock();
            try
            {
                if (_onReceivedMessage != null)
                {
                    if (_onReceivedMessage(msg))
                    {
                        return;
                    }
                }
            }
            finally
            {
                _onReceivedMessageLocker.ExitReadLock();
            }

            PutMessageToReceiveQueue(msg);
        }

        #endregion
    }
}
