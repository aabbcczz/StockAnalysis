namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Create machine tree with greedy intra-pod balanced strategy
    /// </summary>
    public class GreedyIntraPodBalancedMachineTreeBuilder : IMachineTreeBuilder
    {
        #region IMachineTreeBuilder Interface

        /// <summary>
        /// Build machine tree from machine list
        /// </summary>
        /// <param name="machines">list of machine information</param>
        /// <param name="maxFanout">max number of children a node can have</param>
        /// <returns>Created machine tree</returns>
        public MachineTree Build(IEnumerable<WorkerMachineInfo> machines, int maxFanout)
        {
            if (machines == null)
            {
                throw new ArgumentNullException("machines");
            }

            int machineCount = machines.Count();
            if (machineCount < 2)
            {
                throw new ArgumentException("Machine list should contain at least 2 machines");
            }

            if (maxFanout < 2)
            {
                throw new ArgumentException("Max fan-out should be at least 2");
            }

            // check if there are duplicated machine id
            if (!machines.GroupBy(m => m.Id).All(g => g.Count() == 1))
            {
                throw new InvalidOperationException("Duplicated machine ID is detected");
            }

            // check if there are duplicated machine names with the same port
            if (!machines.GroupBy(m => m.FullName).All(g => g.Count() == 1))
            {
                throw new InvalidOperationException("Duplicated machine name and port is detected");
            }

            // calculate tree level
            int totalLevel = this.CalculateTreeLevel(machineCount, maxFanout);
            if (totalLevel <= 1)
            {
                throw new InvalidOperationException("Level of tree must be greater than 1");
            }

            // calculate ideal fan-out
            int idealFanout = this.CalculateIdealFanout(machineCount, maxFanout, totalLevel);

            // calculate node number per level
            int[] nodeNumberPerLevel = new int[totalLevel];
            if (!CalculateNumberOfNodePerLevel(nodeNumberPerLevel, totalLevel, 0, idealFanout, machineCount))
            {
                throw new InvalidOperationException("Failed to calculate the best number of node in each level of tree");
            }

            List<List<WorkerMachineInfo>> machineGroups;

            // check if machine name is IP address (IPv4 or IPv6) or is valid AP machine name
            if (machines.All(m => m.IsAPMachine))
            {
                // group machines by pod and sort groups by the number of machines in the group ascendingly. 
                // Inside each group, machines are ordered by machine ID.
                machineGroups = machines
                    .GroupBy(machine => machine.PodName)
                    .Select(g => g.OrderBy(wmi => wmi.Id).ToList())
                    .ToList();

                machineGroups.Sort((wmiX, wmiY) =>
                    {
                        int comparsion = wmiX.Count.CompareTo(wmiY.Count);
                        if (comparsion == 0)
                        {
                            comparsion = wmiX[0].Id.CompareTo(wmiY[0].Id);
                        }

                        return comparsion;
                    });
            }
            else
            {
                machineGroups = new List<List<WorkerMachineInfo>>();
                machineGroups.Add(machines.OrderBy(wmi => wmi.Id).ToList());
            }

            MachineTree tree = new MachineTree();

            // now we build tree from root. the algorithm is very simple:
            // we always select machines from the smallest pod until all machines
            // in the pod have been used.
            List<int> parents = new List<int>() { MachineTree.EmptyMachineId };
            for (int level = 0; level < totalLevel; ++level)
            {
                List<int> children = new List<int>();

                int parentsCount = parents.Count;
                int childrenCount = nodeNumberPerLevel[level];

                int[] childrenCountPerParent = new int[parentsCount];
                for (int i = 0; i < childrenCountPerParent.Length; ++i)
                {
                    int additionalChild = (i < childrenCount % parentsCount) ? 1 : 0;
                    childrenCountPerParent[i] = childrenCount / parentsCount + additionalChild;
                }

                for (int parentIndex = 0; parentIndex < parentsCount; ++parentIndex)
                {
                    for (int childIndex = 0; childIndex < childrenCountPerParent[parentIndex]; ++childIndex)
                    {
                        WorkerMachineInfo machine = PickNextMachineFromGroups(machineGroups);
                        Debug.Assert(machine != null, "Failed to get next machine when building tree, PickNextMachineFromGroups() returns null");

                        tree.AddRelation(machine.Id, parents[parentIndex]);
                        children.Add(machine.Id);
                    }
                }

                // now children will become the parents of next level
                parents = children;
            }

            // double check if all machines has been assigned to tree.
            if (machineGroups.Sum(g => g.Count) != 0)
            {
                throw new InvalidOperationException("All machines should be assigned to the tree");
            }

            return tree;
        }

        #endregion

        /// <summary>
        /// Picks the next machine from groups.
        /// </summary>
        /// <param name="machines">The list of machine groups. 
        /// Each time the 1st machine of the group will be picked and removed from the group.
        /// The group will be removed if no machines left in the group.</param>
        /// <returns>The selected worker machine info. It will be removed from the original machine list</returns>
        private WorkerMachineInfo PickNextMachineFromGroups(List<List<WorkerMachineInfo>> machines)
        {
            Debug.Assert(machines != null, "machines should not be null");

            while (true)
            {
                if (machines.Count == 0)
                {
                    return null;
                }

                if (machines[0].Count == 0)
                {
                    machines.RemoveAt(0);
                }
                else
                {
                    WorkerMachineInfo machine = machines[0][0];

                    machines[0].RemoveAt(0);

                    return machine;
                }
            }
        }

        // Assume there are totally N machines, the max fan-out of each node is F, the level of tree is K,
        // and the number of nodes in each level i is m[i] (0 <= i < K), then the number of nodes in each level should satisfy:
        // 1. m[0] = 1 (root node)
        // 2. m[i] <= m[i+1]
        // 3. m[i] * F >= m[i+1]
        // 4. m[0] + m[1] + ... + m[k] = N;
        // and the target is to maximize the value SIGMA((m[i]/m[i-1] - F) * m[i - 1]) where 1 <= i < K.
        // So it means the value SIGMA(m[i], 1, K - 1) - F * SIGMA(m[i], 0, K - 2) should be maximized.
        // From 1. and 4., we know SIGMA(m[i], 1, K - 1) = N - 1, so we should minimize SIGMA(m[i], 0, K - 2)
        // under the constraint 1~4.
        /// <summary>
        /// Try to calculate the best number of node in each level of the tree.
        /// </summary>
        /// <param name="nodeNumberPerLevel">the array used for storing the number of node in each level. root is level 0</param>
        /// <param name="totalLevel">the level of tree</param>
        /// <param name="startLevel">the start level that need to be assigned</param>
        /// <param name="maxFanout">the max fan-out of each node</param>
        /// <param name="numberOfUnassignedNode">the number of unassigned node</param>
        /// <returns>true if there is an valid assignment</returns>
        private bool CalculateNumberOfNodePerLevel(int[] nodeNumberPerLevel, int totalLevel, int startLevel, int maxFanout, int numberOfUnassignedNode)
        {
            Debug.Assert(nodeNumberPerLevel != null, "nodeNumberPerLevel should not be null");
            Debug.Assert(nodeNumberPerLevel.Length >= totalLevel, "nodeNumberPerLevel should has length >= total tree level");

            if (startLevel >= totalLevel)
            {
                return false;
            }

            if (startLevel == 0)
            {
                nodeNumberPerLevel[0] = 1;

                return this.CalculateNumberOfNodePerLevel(nodeNumberPerLevel, totalLevel, startLevel + 1, maxFanout, numberOfUnassignedNode - 1);
            }
            else if (startLevel == totalLevel - 1)
            {
                if (numberOfUnassignedNode <= maxFanout * nodeNumberPerLevel[startLevel - 1]
                    && numberOfUnassignedNode >= nodeNumberPerLevel[startLevel - 1])
                {
                    nodeNumberPerLevel[startLevel] = numberOfUnassignedNode;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                int min = nodeNumberPerLevel[startLevel - 1];
                int max = nodeNumberPerLevel[startLevel - 1] * maxFanout;
                for (int i = min; i <= max; i++)
                {
                    nodeNumberPerLevel[startLevel] = i;

                    if (this.CalculateNumberOfNodePerLevel(nodeNumberPerLevel, totalLevel, startLevel + 1, maxFanout, numberOfUnassignedNode - i))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Calculates the tree level.
        /// </summary>
        /// <param name="machineCount">The machine count.</param>
        /// <param name="maxFanout">The max fan-out.</param>
        /// <returns>Total levels required for the tree</returns>
        private int CalculateTreeLevel(int machineCount, int maxFanout)
        {
            Debug.Assert(machineCount > 0, "Machine count should > 0");
            Debug.Assert(maxFanout > 0, "Max fan-out should > 0");

            // calculate tree level. the max number of nodes in a N-level tree is 
            // 1 + fan-out + fan-out^2 + ... + fan-out^(N-1);
            int totalLevel = 1;
            int powOfFanout = 1;
            int totalMachineNumber = 0;
            while (totalMachineNumber + powOfFanout < machineCount)
            {
                totalMachineNumber += powOfFanout;
                totalLevel++;
                powOfFanout *= maxFanout;
            }

            return totalLevel;
        }

        /// <summary>
        /// Calculates the ideal fan-out.
        /// </summary>
        /// <param name="machineCount">The machine count.</param>
        /// <param name="maxFanout">The max fan-out.</param>
        /// <param name="totalLevel">The total level.</param>
        /// <returns>Ideal fan-out for the tree</returns>
        private int CalculateIdealFanout(int machineCount, int maxFanout, int totalLevel)
        {
            int idealFanout;

            for (int i = maxFanout; i > 1; i--)
            {
                int treeNodeCount = 0;
                int fanout = 1;

                for (int j = 0; j < totalLevel; j++)
                {
                    treeNodeCount += fanout;
                    fanout *= i;
                }

                if (treeNodeCount < machineCount)
                {
                    idealFanout = i + 1;
                    return idealFanout;
                }
            }

            idealFanout = 2;
            return idealFanout;
        }
    }
}
