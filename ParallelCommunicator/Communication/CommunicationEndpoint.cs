namespace ParallelFastRank
{
    /// <summary>
    /// Const values representing communication end point.
    /// </summary>
    public static class CommunicationEndPoint
    {
        /// <summary>
        /// Master worker id
        /// </summary>
        public const int MasterWorkerId = 0;

        /// <summary>
        /// The end point representing all workers in current parallel enviroment
        /// </summary>
        public const int ToAllWorkerId = -1;

        /// <summary>
        /// Parallel worker end point of machine tree in hierarchical communication
        /// </summary>
        public const int ToParentWorkerId = -2;

        /// <summary>
        /// The end point representing all children workers of machine tree in hierarchical communication
        /// </summary>
        public const int ToChildrenWorkerId = -3;

        /// <summary>
        /// The end point represeting all workers of a group. If no worker group is defined, it represents all workers in current parallel environment
        /// </summary>
        public const int ToAllWithinGroupWorkerId = -4;
    }
}
