namespace StockAnalysis.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    /// <summary>
    /// This class is used for partitioning data based on data schema, description and time.
    /// </summary>
    public static class DataPartitioner
    {
        private readonly static DateTime minPartitionTimeInclusive = new DateTime(1900, 1, 1);
        private readonly static DateTime maxPartitionTimeExclusive = new DateTime(2100, 1, 1);

        /// <summary>
        /// the zero point of data partition. partition number could be negative 
        /// if real data time is earlier than the zero point.
        /// </summary>
        public readonly static DateTime PartitionZeroPoint = new DateTime(2000, 1, 1); 

        /// <summary>
        /// Partition data
        /// </summary>
        /// <param name="dataDescription">description of data to be partitioned</param>
        /// <param name="startTimeInclusive">the start time (inclusive) of data to be partitioned</param>
        /// <param name="endTimeExclusive">the end time (exclusive) of data to be partitioned</param>
        /// <returns></returns>
        public static IEnumerable<DataPartitionDescription> PartitionData(DataDescription dataDescription, DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            if (dataDescription == null)
            {
                throw new ArgumentNullException();
            }

            if (startTimeInclusive < minPartitionTimeInclusive || endTimeExclusive > maxPartitionTimeExclusive)
            {
                throw new ArgumentOutOfRangeException($"the start time and end time should be in [{minPartitionTimeInclusive}, {maxPartitionTimeExclusive})");
            }

            if (startTimeInclusive >= endTimeExclusive)
            {
                throw new ArgumentException("start time should be earlier than end time");
            }

            switch (dataDescription.Schema)
            {
                case DataSchema.Tick:
                    return PartitionTickData(dataDescription, startTimeInclusive, endTimeExclusive);

                case DataSchema.Bar:
                    return PartitionBarData(dataDescription, startTimeInclusive, endTimeExclusive);

                case DataSchema.Dde:
                    return PartitionDdeData(dataDescription, startTimeInclusive, endTimeExclusive);

                default:
                    throw new NotSupportedException($"schema type {dataDescription.Schema} is not supported");
            }
        }

        private static IEnumerable<DataPartitionDescription> PartitionTickData(DataDescription dataDescription, DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            System.Diagnostics.Debug.Assert(dataDescription != null);
            System.Diagnostics.Debug.Assert(startTimeInclusive >= minPartitionTimeInclusive);
            System.Diagnostics.Debug.Assert(endTimeExclusive <= maxPartitionTimeExclusive);
            System.Diagnostics.Debug.Assert(startTimeInclusive < endTimeExclusive);

            // for tick data, every partition stores only one day data
            return PartitionTimeByDay(startTimeInclusive, endTimeExclusive)
                .Select(tup => new DataPartitionDescription()
                {
                    DataDescription = dataDescription,
                    StartTimeInclusive = tup.Item1,
                    EndTimeExclusive = tup.Item2,
                    PartitionId = string.Format("{0:yyyyMMdd}", tup.Item1)
                });
        }

        private static IEnumerable<DataPartitionDescription> PartitionDdeData(DataDescription dataDescription, DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            System.Diagnostics.Debug.Assert(dataDescription != null);
            System.Diagnostics.Debug.Assert(startTimeInclusive >= minPartitionTimeInclusive);
            System.Diagnostics.Debug.Assert(endTimeExclusive <= maxPartitionTimeExclusive);
            System.Diagnostics.Debug.Assert(startTimeInclusive < endTimeExclusive);

            // for dde data, every partition stores 10 years data
            return PartitionTimeByYear(startTimeInclusive, endTimeExclusive, 10)
                .Select(tup => new DataPartitionDescription()
                {
                    DataDescription = dataDescription,
                    StartTimeInclusive = tup.Item1,
                    EndTimeExclusive = tup.Item2,
                    PartitionId = string.Format("{0:yyyy}", tup.Item1)
                });
        }

        private static IEnumerable<DataPartitionDescription> PartitionBarData(DataDescription dataDescription, DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            System.Diagnostics.Debug.Assert(dataDescription != null);
            System.Diagnostics.Debug.Assert(startTimeInclusive >= minPartitionTimeInclusive);
            System.Diagnostics.Debug.Assert(endTimeExclusive <= maxPartitionTimeExclusive);
            System.Diagnostics.Debug.Assert(startTimeInclusive < endTimeExclusive);

            // for bar data, depends on the granularity G, each partition contains N data. 
            // the map between G and N is following:
            //  G           N
            //  >1 day      all data
            //  >=30 min     10 years
            //  >=5 min      1 year
            //  >=30 sec     1 month
            //  < 30 sec     1 day
            if (dataDescription.Granularity > (uint)DataGranularity.D1d)
            {
                return new DataPartitionDescription[] 
                {
                    new DataPartitionDescription()
                    {
                        DataDescription = dataDescription,
                        StartTimeInclusive = minPartitionTimeInclusive,
                        EndTimeExclusive = maxPartitionTimeExclusive,
                        PartitionId = "0"
                    }
                };
            }
            else if (dataDescription.Granularity >= (uint)DataGranularity.D30min)
            {
                return PartitionTimeByYear(startTimeInclusive, endTimeExclusive, 10)
                    .Select(tup => new DataPartitionDescription()
                    {
                        DataDescription = dataDescription,
                        StartTimeInclusive = tup.Item1,
                        EndTimeExclusive = tup.Item2,
                        PartitionId = string.Format("{0:yyyy}", tup.Item1)
                    });
            }
            else if (dataDescription.Granularity >= (uint)DataGranularity.D5min)
            {
                return PartitionTimeByYear(startTimeInclusive, endTimeExclusive, 1)
                    .Select(tup => new DataPartitionDescription()
                    {
                        DataDescription = dataDescription,
                        StartTimeInclusive = tup.Item1,
                        EndTimeExclusive = tup.Item2,
                        PartitionId = string.Format("{0:yyyy}", tup.Item1)
                    });
            }
            else if (dataDescription.Granularity >= (uint)DataGranularity.D30s)
            {
                return PartitionTimeByMonth(startTimeInclusive, endTimeExclusive)
                    .Select(tup => new DataPartitionDescription()
                    {
                        DataDescription = dataDescription,
                        StartTimeInclusive = tup.Item1,
                        EndTimeExclusive = tup.Item2,
                        PartitionId = string.Format("{0:yyyyMM}", tup.Item1)
                    });
            }
            else
            {
                return PartitionTimeByDay(startTimeInclusive, endTimeExclusive)
                    .Select(tup => new DataPartitionDescription()
                    {
                        DataDescription = dataDescription,
                        StartTimeInclusive = tup.Item1,
                        EndTimeExclusive = tup.Item2,
                        PartitionId = string.Format("{0:yyyyMMdd}", tup.Item1)
                    });
            }
        }

        private static IEnumerable<Tuple<DateTime, DateTime>> PartitionTimeByDay(DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            System.Diagnostics.Debug.Assert(startTimeInclusive >= minPartitionTimeInclusive);
            System.Diagnostics.Debug.Assert(endTimeExclusive <= maxPartitionTimeExclusive);
            System.Diagnostics.Debug.Assert(startTimeInclusive < endTimeExclusive);

            DateTime partitionStartDate = startTimeInclusive.Date;
            DateTime partitionEndDate = endTimeExclusive.Date;
            if (partitionEndDate < endTimeExclusive)
            {
                partitionEndDate.AddDays(1);
            }

            while (partitionStartDate < partitionEndDate)
            {
                var nextDate = partitionStartDate.AddDays(1);

                yield return Tuple.Create(partitionStartDate, nextDate);

                partitionStartDate = nextDate;
            }
        }

        private static IEnumerable<Tuple<DateTime, DateTime>> PartitionTimeByMonth(DateTime startTimeInclusive, DateTime endTimeExclusive)
        {
            System.Diagnostics.Debug.Assert(startTimeInclusive >= minPartitionTimeInclusive);
            System.Diagnostics.Debug.Assert(endTimeExclusive <= maxPartitionTimeExclusive);
            System.Diagnostics.Debug.Assert(startTimeInclusive < endTimeExclusive);

            DateTime partitionStartDate = new DateTime(startTimeInclusive.Date.Year, startTimeInclusive.Date.Month, 1);
            DateTime partitionEndDate = new DateTime(endTimeExclusive.Date.Year, endTimeExclusive.Date.Month, 1);

            if (partitionEndDate < endTimeExclusive)
            {
                partitionEndDate.AddMonths(1);
            }

            while (partitionStartDate < partitionEndDate)
            {
                var nextDate = partitionStartDate.AddMonths(1);

                yield return Tuple.Create(partitionStartDate, nextDate);

                partitionStartDate = nextDate;
            }
        }

        private static IEnumerable<Tuple<DateTime, DateTime>> PartitionTimeByYear(DateTime startTimeInclusive, DateTime endTimeExclusive, int years)
        {
            if (years <= 0)
            {
                throw new ArgumentException("years must be greater than 0");
            }

            System.Diagnostics.Debug.Assert(startTimeInclusive >= minPartitionTimeInclusive);
            System.Diagnostics.Debug.Assert(endTimeExclusive <= maxPartitionTimeExclusive);
            System.Diagnostics.Debug.Assert(startTimeInclusive < endTimeExclusive);

            int partitionStartYear = (startTimeInclusive.Year / years) * years;
            DateTime partitionStartDate = new DateTime(partitionStartYear, 1, 1);

            int partitionEndYear = (endTimeExclusive.Year / years) * years;
            DateTime partitionEndDate = new DateTime(partitionEndYear, 1, 1);
            if (partitionEndDate < endTimeExclusive)
            {
                partitionEndDate.AddYears(years);
            }

            while (partitionStartDate < partitionEndDate)
            {
                var nextDate = partitionStartDate.AddYears(years);

                yield return Tuple.Create(partitionStartDate, nextDate);

                partitionStartDate = nextDate;
            }
        }
    }
}
