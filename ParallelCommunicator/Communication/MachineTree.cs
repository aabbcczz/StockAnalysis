namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Topologic structure maintaining relationship of worker machines
    /// </summary>
    public sealed class MachineTree : IEquatable<MachineTree>
    {
        public const int EmptyMachineId = -1;

        #region Private Variables

        private readonly Dictionary<int, int> _machineToParentMap = new Dictionary<int, int>();
        private readonly Dictionary<int, List<int>> _machineToChildrenMap = new Dictionary<int, List<int>>();
        private readonly List<int> _emptyList = new List<int>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the total count of machines in the tree
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }

        #endregion
         
        #region Public Methods

        /// <summary>
        /// Adds the relation to the tree
        /// </summary>
        /// <param name="machineId">The machine id.</param>
        /// <param name="parentMachineId">The parent machine id.</param>
        /// <exception cref="System.ArgumentException">
        /// Cannot add empty machine in the tree
        /// or
        /// MachineId should not equal to parentMachineId
        /// or
        /// Machine  + machineId +  is duplicated
        /// </exception>
        public void AddRelation(int machineId, int parentMachineId)
        {
            if (machineId == EmptyMachineId)
            {
                throw new ArgumentException("Cannot add empty machine in the tree");    
            }

            if (machineId == parentMachineId)
            {
                throw new ArgumentException("MachineId should not equal to parentMachineId");
            }

            if (this.Exists(machineId))
            {
                throw new ArgumentException("Machine " + machineId + " is duplicated");
            }
            
            // Increase count only if machineId is not parent or child of an existing machine
            if (machineId != EmptyMachineId && !this._machineToParentMap.ContainsKey(machineId) && !this._machineToChildrenMap.ContainsKey(machineId))
            {
                this.Count++;
            }

            // Increase count again if parentMachineId is not parent of child of an existing machine
            if (parentMachineId != EmptyMachineId && !this._machineToParentMap.ContainsKey(parentMachineId) && !this._machineToChildrenMap.ContainsKey(parentMachineId))
            {
                this.Count++;
            }

            // add the relation
            this._machineToParentMap.Add(machineId, parentMachineId);
            if (!this._machineToChildrenMap.ContainsKey(parentMachineId))
            {
                this._machineToChildrenMap[parentMachineId] = new List<int>();
            }

            this._machineToChildrenMap[parentMachineId].Add(machineId);
        }

        /// <summary>
        /// Checks if the specified machine exists
        /// </summary>
        /// <param name="machineId">The machine id.</param>
        /// <returns>true if exists, false otherwise</returns>
        public bool Exists(int machineId)
        {
            return this._machineToParentMap.ContainsKey(machineId);
        }

        /// <summary>
        /// Gets the parent of the given machine
        /// </summary>
        /// <param name="machineId">The machine id.</param>
        /// <returns>
        /// Parent id of the machine
        /// </returns>
        /// <exception cref="System.ArgumentException">Machine  + machineId +  does not exist</exception>
        public int GetParent(int machineId)
        {
            if (!this.Exists(machineId))
            {
                throw new ArgumentException("Machine " + machineId + " does not exist");
            }

            return this._machineToParentMap[machineId];
        }

        /// <summary>
        /// Gets the children of given machine
        /// </summary>
        /// <param name="machineId">The machine id.</param>
        /// <returns>
        /// A list of children machine's id
        /// </returns>
        /// <exception cref="System.ArgumentException">Machine  + machineId +  does not exist</exception>
        public IEnumerable<int> GetChildren(int machineId)
        {
            if (!this._machineToChildrenMap.ContainsKey(machineId))
            {
                return _emptyList;
            }

            return this._machineToChildrenMap[machineId];
        }

        /// <summary>
        /// Gets the offsprings of the given machine
        /// </summary>
        /// <param name="machineId">The machine id.</param>
        /// <returns>A list of offspring's id</returns>
        public IEnumerable<int> GetOffsprings(int machineId)
        {
            IEnumerable<int> children = this.GetChildren(machineId);

            return children.SelectMany(this.GetOffsprings).Concat(children);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var kvp in this._machineToParentMap)
            {
                builder.AppendFormat("{0},{1}", kvp.Key, kvp.Value);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public override int GetHashCode()
        {
            return this._machineToParentMap.GetHashCode() ^ this._machineToChildrenMap.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MachineTree))
            {
                return false;
            }

            return this.Equals(obj as MachineTree);
        }

        #region IEquatable<MachineTree> Interface

        public bool Equals(MachineTree other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.Count != other.Count
                || this._machineToParentMap.Count != other._machineToParentMap.Count
                || this._machineToChildrenMap.Count != other._machineToChildrenMap.Count)
            {
                return false;
            }

            foreach (KeyValuePair<int, int> kvp in this._machineToParentMap)
            {
                if (!other._machineToParentMap.ContainsKey(kvp.Key))
                {
                    return false;
                }

                if (kvp.Value != other._machineToParentMap[kvp.Key])
                {
                    return false;
                }
            }

            foreach (KeyValuePair<int, List<int>> kvp in this._machineToChildrenMap)
            {
                if (!other._machineToChildrenMap.ContainsKey(kvp.Key))
                {
                    return false;
                }

                List<int> thisValues = kvp.Value;
                List<int> otherValues = other._machineToChildrenMap[kvp.Key];
                if (thisValues == null && otherValues == null)
                {
                    continue;
                }

                if (thisValues == null ^ otherValues == null || thisValues.Count != otherValues.Count)
                {
                    return false;
                }

                thisValues.Sort();
                otherValues.Sort();
                for (int i = 0; i < thisValues.Count; i++)
                {
                    if (thisValues[i] != otherValues[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Serializes the machine tree to specified stream
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Serialize(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (var kvp in _machineToParentMap)
                {
                    writer.WriteLine("{0},{1}", kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// Deserializes the machine tree from specified stream
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>Machine tree created according to the one serialized in stream</returns>
        /// <exception cref="System.IO.InvalidDataException">Input line [ + line + ] is not valid: each line should have two parts</exception>
        /// <exception cref="System.FormatException">Input line [ + line + ] is not valid: each part should be a non-negative integer (except root node can have -1 as parentId)</exception>
        public MachineTree Deserialize(Stream stream)
        {
            this._machineToParentMap.Clear();
            this._machineToChildrenMap.Clear();

            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] tokens = line.Split(',');
                    if (tokens.Length != 2)
                    {
                        throw new InvalidDataException("Input line [" + line + "] is not valid: each line should have two parts");
                    }

                    int machineId;
                    int parentId;

                    if (!Int32.TryParse(tokens[0], out machineId)
                        || !Int32.TryParse(tokens[1], out parentId)
                        || machineId < 0
                        || parentId < 0 && parentId != EmptyMachineId)
                    {
                        throw new FormatException("Input line [" + line + "] is not valid: each part should be a non-negative integer (except root node can have -1 as parentId)");
                    }

                    this.AddRelation(machineId, parentId);
                }
            }

            return this;
        }

        public static bool operator ==(MachineTree lhsTree, MachineTree rhsTree)
        {
            if (Object.ReferenceEquals(lhsTree, null))
            {
                return Object.ReferenceEquals(rhsTree, null);
            }

            return lhsTree.Equals(rhsTree);
        }

        public static bool operator !=(MachineTree lhsTree, MachineTree rhsTree)
        {
            return !(lhsTree == rhsTree);
        }

        #endregion
    }
}