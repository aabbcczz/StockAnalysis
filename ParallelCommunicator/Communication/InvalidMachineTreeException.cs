namespace ParallelFastRank
{
    using System;

    /// <summary>
    /// The machine tree is invalid
    /// </summary>
    public class InvalidMachineTreeException : Exception
    {
        public InvalidMachineTreeException(string message)
            : base(message)
        {
        }
    }
}
