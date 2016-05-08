namespace ParallelFastRank
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for machine tree builder
    /// </summary>
    public interface IMachineTreeBuilder
    {
        /// <summary>
        /// Build machine tree from machine list
        /// </summary>
        /// <param name="machines">collection of machine information</param>
        /// <param name="maxFanout">max number of children a node can have</param>
        /// <returns>Created machine tree</returns>
        MachineTree Build(IEnumerable<WorkerMachineInfo> machines, int maxFanout);
    }
}
