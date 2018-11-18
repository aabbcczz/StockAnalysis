namespace StockAnalysis.DataAccess
{
    using System;

    /// <summary>
    /// description of data partition
    /// </summary>
    public class DataPartitionDescription
    {
        /// <summary>
        /// the description of data which is stored in the partition
        /// </summary>
        public DataDescription DataDescription { get; set; }

        /// <summary>
        /// the start time of the partition, which is inclusive
        /// </summary>
        public DateTime StartTimeInclusive { get; set; }

        /// <summary>
        /// the end time of the partition, which is exclusive
        /// </summary>
        public DateTime EndTimeExclusive { get; set; }

        /// <summary>
        /// the id of data partition
        /// </summary>
        public string PartitionId { get; set; }
    }
}
