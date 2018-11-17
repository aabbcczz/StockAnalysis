namespace DataAccess
{
    public class DataDescription
    {
        /// <summary>
        /// data category, such as stock, future, stock option, etc.
        /// </summary>
        public DataCategory Category { get; set; }
        /// <summary>
        /// The way how data is repriced.
        /// </summary>
        public RepricingRight RepricingRight { get; set; }
        /// <summary>
        /// data schema, e.g. tick, bar or other kind of data.
        /// </summary>
        public DataSchema Schema { get; set; }
        /// <summary>
        /// the granularity (in second) for specific schema. Currently it is meaningful for Bar, DDE only.
        /// </summary>
        public uint Granularity { get; set; }
    }
}
