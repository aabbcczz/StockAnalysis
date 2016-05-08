namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Build machine tree with the given dump file
    /// </summary>
    public class ManualMachineTreeBuilder : IMachineTreeBuilder
    {
        private readonly string _dumpFile;

        public ManualMachineTreeBuilder(string dumpFile)
        {
            if (String.IsNullOrWhiteSpace(dumpFile))
            {
                throw new ArgumentNullException("dumpFile");
            }

            this._dumpFile = dumpFile;
        }

        #region IMachineTreeBuilder Interface

        /// <summary>
        /// Build machine tree from machine list
        /// </summary>
        /// <param name="machines">list of machine information</param>
        /// <param name="maxFanout">[ignored in this builder] max number of children a node can have</param>
        /// <returns>
        /// Created machine tree
        /// </returns>
        public MachineTree Build(IEnumerable<WorkerMachineInfo> machines, int maxFanout)
        {
            if (machines == null)
            {
                throw new ArgumentNullException("machines");
            }

            if (!File.Exists(_dumpFile))
            {
                throw new FileNotFoundException(_dumpFile);
            }

            MachineTree machineTree = new MachineTree();

            using (FileStream fs = File.OpenRead(_dumpFile))
            {
                machineTree.Deserialize(fs);
            }

            int machineCount = machines.Count();
            if (machineTree.Count != machineCount)
            {
                throw new InvalidMachineTreeException(String.Format("Machine count not as expected. Expect: {0}, Actual: {1}", machineTree.Count, machineCount));
            }

            WorkerMachineInfo invalidWokerMachineInfo = machines.FirstOrDefault(wmi => !machineTree.Exists(wmi.Id));
            if (invalidWokerMachineInfo != null)
            {
                throw new InvalidMachineTreeException(String.Format("Machine {0} not found in the machine tree.", invalidWokerMachineInfo));
            }

            return machineTree;
        }

        #endregion
    }
}
