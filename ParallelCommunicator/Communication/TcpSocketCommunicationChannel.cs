namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using FastRank;

    public sealed class TcpSocketCommunicationChannel : ICommunicationChannel
    {
        #region Private Variables

        private const int RetryIntervalInSeconds = 60; // retry interval is 60 second
        private const int MaxTryTimesForConnectionToMaster = 2880; // retry at most 2880 times (2 days if retry interval is 60 seconds)

        private int _workerId;
        private int _workerCount;

        // worker id's in the group this worker belongs to
        private int[] _workerGroup;

        // the worker index in the worker group.
        // for example, if the current worker id is 3, and it is in a worker group [2,3,6],
        // the local worker id of current worker is 1.
        private int _workerIndexInGroup;

        private int _port;

        private bool _disposed;
        private Thread _listenTread; // thread for listening
        private string _preferredListeningAddress = null;

        // processors to handle communication from remote to local
        private Dictionary<IncomingTcpSocketProcessor, string> _remoteToLocalProcessors;
        private WorkerCommunicator _associatedCommunicator;
        private WorkerMachineInfo[] _workerMachineList;

        // processor to handle communication from local to remote server
        private ThreadSafeOutcomingTcpSocketProcessor[] _localToRemoteProcessors; 

        // data pending for processing.
        private WaitableConcurrentQueue<DataReceivedEventArgs> _pendingData 
            = new WaitableConcurrentQueue<DataReceivedEventArgs>();

        // threads for convert received data to messages.
        private Thread[] _dataProcessThreads = null;

        private CancellationTokenSource _disposingNotifier = new CancellationTokenSource();

        #endregion

        public event EventHandler<DataProcessExceptionEventArgs> ExceptionHandler = null;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocketCommunicationChannel"/> class.
        /// </summary>
        /// <param name="port">The listening point.</param>
        /// <param name="workerCount">The total worker count in current parallel enviroment.</param>
        /// <param name="thisWorkerId">The current worker id.</param>
        /// <param name="workerGroup">The worker id's of the worker group this worker belongs to. Null means all workers in parallel environment is in a single worker group</param>
        /// <param name="workerIndexInGroup">The worker index in the worker group. for example, if the current worker id is 3, and it is in a worker group [2,3,6], the local worker id of current worker is 1.</param>
        /// <param name="preferredListeningAddress">The preferred listening address.</param>
        /// <param name="receiveThreadCount">The number of threads for receiving messages.</param>
        public TcpSocketCommunicationChannel(
            int port, 
            int workerCount, 
            int thisWorkerId, 
            int[] workerGroup = null,
            int workerIndexInGroup = -1,
            string preferredListeningAddress = null, 
            int receiveThreadCount = 1)
        {
            if (thisWorkerId < 0 || thisWorkerId >= workerCount)
            {
                throw new ArgumentOutOfRangeException("thisWorkerId", String.Format("The expected range of worker id should be between [{0}, {1})", 0, workerCount));
            }

            if (workerGroup != null)
            {
                if (!workerGroup.Contains(thisWorkerId))
                {
                    throw new ArgumentException(String.Format("The worker id {0} is not found in the worker group", thisWorkerId), "thisWorkerId");
                }

                if (workerIndexInGroup < 0 || workerIndexInGroup >= workerGroup.Length)
                {
                    throw new ArgumentOutOfRangeException("localWorkerId", String.Format("The expected range of local worker id should be between [{0}, {1})", 0, workerGroup.Length));
                }
            }

            _workerCount = workerCount;
            _workerId = thisWorkerId;
            _workerGroup = workerGroup;
            _workerIndexInGroup = workerIndexInGroup;

            _port = port;
            _disposed = false;

            _remoteToLocalProcessors = new Dictionary<IncomingTcpSocketProcessor, string>();
            _localToRemoteProcessors = null;

            _preferredListeningAddress = preferredListeningAddress;
            _listenTread = new Thread(Listen);
            _listenTread.Name = "TcpSocketListener";
            _listenTread.Start();

            if (receiveThreadCount < 1)
            {
                receiveThreadCount = 1;
            }

            _dataProcessThreads = new Thread[receiveThreadCount];
            for (int i = 0; i < _dataProcessThreads.Length; ++i)
            {
                _dataProcessThreads[i] = new Thread(ProcessData);
                _dataProcessThreads[i].Name = "Data Process " + i.ToString();
                _dataProcessThreads[i].Start();
            }
        }

        #endregion

        #region Public Methods

        public static TcpClient CreateTcpClientBindToLocal(
            string localMachineName,
            string remoteMachineName,
            int remotePort)
        {
            IPAddress address;

            if (!IPAddress.TryParse(localMachineName, out address))
            {
                IPHostEntry entry = Dns.GetHostEntry(localMachineName);
                foreach (var addr in entry.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        address = addr;
                        break;
                    }
                }
            }

            IPEndPoint localEndPoint = new IPEndPoint(address, 0);
            TcpClient client = new TcpClient(localEndPoint);

            try
            {
                client.Connect(remoteMachineName, remotePort);
            }
            catch
            {
                client.Close();
                throw;
            }

            return client;
        }

        public void InitializeAsMaster(WorkerCommunicator communicator, string localMachineName)
        {
            if (_workerId != CommunicationEndPoint.MasterWorkerId ||
                _workerCount != communicator.TotalWorkerCount)
            {
                throw new ArgumentException("Worker Information not correct");
            }

            StaticRuntimeContext.Stdout1.WriteLine("Initialize as Master: total worker count = {0}", _workerCount);

            _associatedCommunicator = communicator;
            _localToRemoteProcessors = new ThreadSafeOutcomingTcpSocketProcessor[_workerCount];
            _workerMachineList = new WorkerMachineInfo[_workerCount];

            // set master machine info.
            _localToRemoteProcessors[CommunicationEndPoint.MasterWorkerId] = null;
            _workerMachineList[CommunicationEndPoint.MasterWorkerId] = new WorkerMachineInfo(localMachineName, _port, _workerId);

            // get worker machine list
            for (int n = 0; n < _workerCount - 1; n++)
            {
                WorkerMachineInfoMessage msg;

                if (CommunicationTimeout.ConnectionTimeout < 0)
                {
                    msg = (WorkerMachineInfoMessage)communicator.ReceiveMessage(MessageType.WorkerMachineInfo);
                }
                else
                {
                    msg = (WorkerMachineInfoMessage)communicator.ReceiveMessage(
                        MessageType.WorkerMachineInfo, 
                        CommunicationTimeout.ConnectionTimeout);
                }

                WorkerMachineInfo worker = msg.MachineInfo;

                // check if there is duplicated worker id.
                if (_workerMachineList[worker.Id] != null)
                {
                    throw new InvalidOperationException("Found duplicated worker id " + worker.Id);
                }

                _workerMachineList[worker.Id] = worker;
                _localToRemoteProcessors[worker.Id] = new ThreadSafeOutcomingTcpSocketProcessor();
                _localToRemoteProcessors[worker.Id].Initialize(localMachineName, worker.Name, worker.Port);
                _localToRemoteProcessors[worker.Id].ExceptionEvent += OnException;

                StaticRuntimeContext.Stdout1.WriteLine("Register worker {0} - {1}:{2}", worker.Id, worker.Name, worker.Port);
            }

            // send work machine list to all workers
            MessagePackage workerMachineListMessage = new WorkerMachineListMessage(_workerMachineList);
            MessagePackageCreatorBase.SetMasterBroadcastMessage(workerMachineListMessage);
            communicator.SendMessage(workerMachineListMessage);

            StaticRuntimeContext.Stdout1.WriteLine("Master worker started.");
        }

        public void InitializeAsWorker(
            string masterMachine, 
            int masterPort,
            WorkerCommunicator communicator, 
            string localMachineName)
        {
            if (_workerId != communicator.WorkerId ||
                _workerCount != communicator.TotalWorkerCount)
            {
                throw new ArgumentException("Worker Information not correct");
            }

            StaticRuntimeContext.Stdout1.WriteLine(
                "Initialize as Worker {1}: total worker count = {0}", 
                _workerCount, 
                _workerId);

            _associatedCommunicator = communicator;
            _localToRemoteProcessors = new ThreadSafeOutcomingTcpSocketProcessor[_workerCount];
            _workerMachineList = new WorkerMachineInfo[_workerCount];

            // init master sender
            StaticRuntimeContext.Stdout1.WriteLine("Initialize master sender: {0}:{1}.", masterMachine, masterPort);
            _localToRemoteProcessors[CommunicationEndPoint.MasterWorkerId] = new ThreadSafeOutcomingTcpSocketProcessor();
            _localToRemoteProcessors[CommunicationEndPoint.MasterWorkerId].Initialize(
                    localMachineName, 
                    masterMachine, 
                    masterPort, 
                    MaxTryTimesForConnectionToMaster,
                    RetryIntervalInSeconds);

            _localToRemoteProcessors[CommunicationEndPoint.MasterWorkerId].ExceptionEvent += OnException;
            _workerMachineList[CommunicationEndPoint.MasterWorkerId] = new WorkerMachineInfo(
                masterMachine, 
                masterPort, 
                CommunicationEndPoint.MasterWorkerId);

            // init current worker sender
            _localToRemoteProcessors[_workerId] = null;
            _workerMachineList[_workerId] = new WorkerMachineInfo(localMachineName, _port, _workerId);

            StaticRuntimeContext.Stdout1.WriteLine(
                "Register current worker {0} - {1}:{2}.",
                _workerId, 
                _workerMachineList[_workerId].Name, 
                _workerMachineList[_workerId].Port);

            // send worker machine information to Master
            MessagePackage msg = new WorkerMachineInfoMessage(_workerMachineList[_workerId]);
            MessagePackageCreatorBase.SetMasterAggregationMessage(_workerId, msg);
            communicator.SendMessage(msg);

            // receive worker machine list from Master
            WorkerMachineListMessage msgs;

            if (CommunicationTimeout.ConnectionTimeout < 0)
            {
                msgs = (WorkerMachineListMessage)communicator.ReceiveMessage(MessageType.WorkerMachineList);
            }
            else
            {
                msgs = (WorkerMachineListMessage)communicator.ReceiveMessage(
                    MessageType.WorkerMachineList, 
                    CommunicationTimeout.ConnectionTimeout);
            }

            WorkerMachineInfo[] workerList = msgs.MachineList;

            // check worker count
            if (workerList.Length != _workerCount)
            {
                throw new InvalidOperationException("Worker list count not correct");
            }

            foreach (WorkerMachineInfo worker in workerList)
            {
                int id = worker.Id;

                // skip master machine and current worker machine
                if (id == CommunicationEndPoint.MasterWorkerId
                    || id == _workerId)
                {
                    continue;
                }

                // check duplicated worker id
                if (_localToRemoteProcessors[id] != null)
                {
                    throw new InvalidOperationException("found duplicated worker id " + id);
                }

                _localToRemoteProcessors[id] = new ThreadSafeOutcomingTcpSocketProcessor();
                _localToRemoteProcessors[id].Initialize(localMachineName, worker.Name, worker.Port);
                _localToRemoteProcessors[id].ExceptionEvent += OnException;

                _workerMachineList[id] = worker;

                StaticRuntimeContext.Stdout1.WriteLine("Register worker {0} - {1}:{2}", id, worker.Name, worker.Port);
            }

            StaticRuntimeContext.Stdout1.WriteLine("Worker {0} started.", _workerId);
        }

        #region ICommunicationChannel Members

        /// <summary>
        /// The action that need help in other threads if they can. (try process pending data)
        /// </summary>
        public Action ActionNeedHelp
        {
            get
            {
                return this.TryProcessData;
            }
        }

        /// <summary>
        /// Gets the status of the communicator (always empty)
        /// </summary>
        public string Status
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="message">the message to be sent</param>
        public void Send(MessagePackage message)
        {
            switch (message.ToId)
            {
                case CommunicationEndPoint.ToAllWorkerId:
                    using (FastRank.Timer.Time(TimerEvent.NetBroadcast))
                    {
                        this.Broadcast(_workerId, message);
                    }

                    break;
                case CommunicationEndPoint.ToParentWorkerId:
                    if (!this._associatedCommunicator.IsHierarchicalCommunicationEnabled)
                    {
                        throw new NotSupportedException("Hierarchical communication is not enabled");
                    }

                    using (FastRank.Timer.Time(TimerEvent.NetSending))
                    {
                        this.Send(this._associatedCommunicator.ParentWorkerId, message);
                    }

                    break;
                case CommunicationEndPoint.ToChildrenWorkerId:
                    if (!this._associatedCommunicator.IsHierarchicalCommunicationEnabled)
                    {
                        throw new NotSupportedException("Hierarchical communication is not enabled");
                    }

                    if (this._associatedCommunicator.ChildWorkerIds == null)
                    {
                        throw new InvalidOperationException("No child workers specified in associated communicator.");
                    }

                    if (this._associatedCommunicator.ChildWorkerIds.Length > 0)
                    {
                        using (FastRank.Timer.Time(TimerEvent.NetBroadcast))
                        {
                            foreach (int id in this._associatedCommunicator.ChildWorkerIds)
                            {
                                this.Send(id, message);
                            }
                        }    
                    }

                    break;

                case CommunicationEndPoint.ToAllWithinGroupWorkerId:
                    using (FastRank.Timer.Time(TimerEvent.NetBroadcast))
                    {
                        this.Broadcast(_workerGroup == null ? _workerId : _workerIndexInGroup, message, _workerGroup);
                    }

                    break;

                default:
                    // the destination is not a special tag, it is just a concrete worker Id
                    using (FastRank.Timer.Time(TimerEvent.NetSending))
                    {
                        this.Send(message.ToId, message);
                    }

                    break;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _disposed = true;

                    // close senders
                    foreach (var processor in _localToRemoteProcessors)
                    {
                        if (processor != null)
                        {
                            processor.Close();
                        }
                    }

                    foreach (var processor in _remoteToLocalProcessors)
                    {
                        processor.Key.Close();
                    }

                    // Killed the listening thread
                    if (_listenTread != null && _listenTread.IsAlive)
                    {
                        _listenTread.Abort();
                    }

                    // stop data processing threads
                    _disposingNotifier.Cancel();
                    foreach (var thread in _dataProcessThreads)
                    {
                        thread.Join();
                    }

                    _disposingNotifier.Dispose();

                    _pendingData.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion

        #region Private Methods

        private static void DoStatistics(MessagePackage message, IList<ArraySegment<byte>> buffers)
        {
            int size = buffers.Sum(buffer => buffer.Count);
            MessageCommunicationStatistics.RoundSendingStatus.AddMessage(message.Type, size);
            MessageCommunicationStatistics.GlobalSendingStatus.AddMessage(message.Type, size);
        }

        private void OnException(object sender, DataProcessExceptionEventArgs args)
        {
            SocketProcessorBase processor = (SocketProcessorBase)sender;
            processor.Dispose();

            if (ExceptionHandler != null)
            {
                try
                {
                    ExceptionHandler(this, args);
                }
                catch (Exception ex)
                {
                    // Exit if any exception in exception handler is thrown out.
                    StaticRuntimeContext.Stderr.WriteLine("Exception occurred in exception handler: {0}", ex);
                    Environment.Exit(-1);

                    throw;
                }
            }
            else
            {
                StaticRuntimeContext.Stderr.WriteLine(
                    "Exception occurred in communication channel: {0}, context: {1}",
                    args.Exception.ToString(),
                    args.Context);

                throw args.Exception;
            }
        }

        private void Listen()
        {
            StaticRuntimeContext.Stdout1.WriteLine("Start Tcp server, listening on port {0}", _port);

            IPAddress ipaddress;
            if (_preferredListeningAddress != null)
            {
                if (!IPAddress.TryParse(_preferredListeningAddress, out ipaddress))
                {
                    StaticRuntimeContext.Stderr.WriteLine(
                        "Invalid preferred listening address : {0}. please provide a valid IP address", 
                        _preferredListeningAddress);
                    ipaddress = IPAddress.Any;
                }
            }
            else
            {
                ipaddress = IPAddress.Any;
            }

            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start(Math.Max(5, _workerCount));

            while (!_disposed)
            {
                try
                {
                    // Accept a new connection from the net, blocking till one comes in
                    TcpClient client = listener.AcceptTcpClient();

                    IncomingTcpSocketProcessor processor = new IncomingTcpSocketProcessor(client);

                    try
                    {
                        processor.ReceivedDataEvent += OnReceivedData;
                        processor.ExceptionEvent += OnException;

                        lock (_remoteToLocalProcessors)
                        {
                            _remoteToLocalProcessors.Add(processor, string.Empty);
                        }
                    }
                    catch
                    {
                        processor.Dispose();
                        throw;
                    }
                }
                catch (NullReferenceException)
                {
                    // Sometimes the call to AcceptTcpClient() throw NullReferenceException, and it 
                    // can go on accept client connections. We don't find the solution, so just put 
                    // a workaround here.
                    // do nothing, accept failed
                    StaticRuntimeContext.Stderr.WriteLine(
                        "A null reference exception was thrown, presumably by the socket listener.");
                }
                catch (Exception e)
                {
                    StaticRuntimeContext.Stderr.WriteLine(
                        "Exception thrown while trying to listen for connections: " + e.ToString());

                    throw;
                }
            }

            listener.Stop();
        }
        
        private void OnReceivedData(object sender, DataReceivedEventArgs eventArgs)
        {
            _pendingData.Add(eventArgs);
        }

        private void ProcessData(DataReceivedEventArgs args)
        {
            args.Timer.Dispose();
            args.Timer = null;

            int offset = args.ReceivedDataBuffer.Offset;
            int length = args.ReceivedDataBuffer.Offset + args.ReceivedDataBuffer.Count;

            while (offset < length)
            {
                int oldOffset = offset;
                MessagePackage message = MessagePackage.DeserializeMessage(
                    args.ReceivedDataBuffer.Array,
                    length,
                    ref offset);

                // set receive time ticks and count timer
                message.ReceiveTimeTicks = TimeSynchronizer.GetSynchronizedTicksForMessage(message.Type);
                MessageTimeCollector.CollectMessageDataGlobally(message);

                MessageCommunicationStatistics.RoundReceiveStatus.AddMessage(message.Type, offset - oldOffset);
                MessageCommunicationStatistics.GlobalReceiveStatus.AddMessage(message.Type, offset - oldOffset);

                _associatedCommunicator.Receive(message);
            }

            if (args.ReceivedDataCallback != null)
            {
                args.ReceivedDataCallback(args.CallbackState);
            }
        }

        private void TryProcessData()
        {
            if (!_disposingNotifier.IsCancellationRequested)
            {
                DataReceivedEventArgs args;

                if (_pendingData.TryTake(out args))
                {
                    ProcessData(args);
                }
            }
        }

        private void ProcessData()
        {
            while (!_disposingNotifier.IsCancellationRequested)
            {
                try
                {
                    DataReceivedEventArgs args = _pendingData.Take(_disposingNotifier.Token);

                    ProcessData(args);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
            }
        }

        private void SentMessageCallback(object state)
        {
            SendMessageState sms = (SendMessageState)state;

            DoStatistics(sms.Message, sms.Buffers);

            int remainReferenceCount = sms.DecrementReference();

            if (remainReferenceCount == 0)
            {
                MessagePackage.FreeSerializationMemory(sms.Buffers);
                SendMessageState.Free(sms);
            }
            else if (remainReferenceCount < 0)
            {
                throw new InvalidOperationException("message reference count is smaller than zero");
            }
        }

        /// <summary>
        /// Sends the message to specific receiver
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">id is not valid</exception>
        private void Send(int id, MessagePackage message)
        {
            if (id >= _workerCount || id < 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }

            if (id == _workerId)
            {
                // send to self
                _associatedCommunicator.Receive(message);
            }
            else
            {
                // set send time ticks;
                message.SendTimeTicks = TimeSynchronizer.GetSynchronizedTicksForMessage(message.Type);

                // send message
                IList<ArraySegment<byte>> buffers = MessagePackage.SeralizeMessage(message);
                SendMessageState stateObj = SendMessageState.Allocate();
                stateObj.Message = message;
                stateObj.Buffers = buffers;
                stateObj.ReferenceCount = 1;

                _localToRemoteProcessors[id].SendData(buffers, SentMessageCallback, stateObj);
            }
        }

        /// <summary>
        /// Broadcasts the message. 
        /// If the worker group is not provided, the message will be sent to all workers and the source id is the global worker id;
        /// If the worker group is provided, the message will only be sent to workers in the group and the source id is the index of the worker group array.
        /// </summary>
        /// <param name="id">The broadcast source id.</param>
        /// <param name="message">The message.</param>
        /// <param name="workerGroup">The worker group.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">id is invalid</exception>
        private void Broadcast(int id, MessagePackage message, int[] workerGroup = null)
        {
            int workerCount = (workerGroup == null || workerGroup.Length == 0) ? _workerCount : workerGroup.Length;

            if (id >= workerCount || id < 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }

            // set send time ticks;
            message.SendTimeTicks = TimeSynchronizer.GetSynchronizedTicksForMessage(message.Type);

            // send sequence: id + 1, id + 2, ..., workerCount - 1, 0, 1, ..., id - 1, id
            int nextWorkerId = (id + 1) % workerCount;

            IList<ArraySegment<byte>> buffers = MessagePackage.SeralizeMessage(message);
            SendMessageState stateObj = SendMessageState.Allocate();
            stateObj.Message = message;
            stateObj.Buffers = buffers;
            stateObj.ReferenceCount = workerCount;

            while (nextWorkerId != id)
            {
                int machineId = workerGroup == null ? nextWorkerId : workerGroup[nextWorkerId];

                _localToRemoteProcessors[machineId].SendData(buffers, SentMessageCallback, stateObj);

                nextWorkerId = (nextWorkerId + 1) % workerCount;
            }

            // send to self
            _associatedCommunicator.Receive(message);
        }

        #endregion

        #region Nested Types

        private sealed class SendMessageState
        {
            private static ConcurrentObjectPool<SendMessageState> pool
                = new ConcurrentObjectPool<SendMessageState>(() => { return new SendMessageState(); });

            private int _referenceCount = 0;

            public SendMessageState()
            {
                Buffers = null;
            }

            public int ReferenceCount
            {
                get { return _referenceCount; }
                set { _referenceCount = value; }
            }

            public IList<ArraySegment<byte>> Buffers { get; set; }

            public MessagePackage Message { get; set; }

            public static SendMessageState Allocate()
            {
                SendMessageState stateObj = pool.TakeObject();

                if (stateObj == null)
                {
                    throw new InvalidOperationException("allocate SendMessageState object failed");
                }

                return stateObj;
            }

            public static void Free(SendMessageState state)
            {
                pool.ReturnObject(state);
            }

            public int DecrementReference()
            {
                return Interlocked.Decrement(ref _referenceCount);
            }
        }

        #endregion
    }
}
