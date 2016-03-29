namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using FastRank;

    /// <summary>
    /// This class is used for synchronizing the time of local machine with the time in the server
    /// so that the timestamp in messages can be used for calculating latency.
    /// Theoretically, we can use NTP (Network Time Protocol) to get more precise estimation of 
    /// time difference between machines, but in reality, the time difference between machines are 
    /// generally in 10 millisecond level, and NTP can't get better resolution than 100 milliseconds.
    /// So here we use the very simple solution: just use the first worker's time as global time
    /// and ignore the latency of sending/receiving message. From statistic point of view, if we
    /// have enough data, we can roughly understand the latency between machines by substract the
    /// lowest message latency from average message latency.
    /// </summary>
    public static class TimeSynchronizer
    {
        private static Stopwatch watch;

        static TimeSynchronizer()
        {
            RoundTripTimeInMillisecond = 0.0;
            OffsetDateTimeTicksWithServer = 0;

            watch = new Stopwatch();
            watch.Start();
        }

        /// <summary>
        /// The round trip time of sending message between local machine and the server.
        /// It is always 0.0 in current implementation.
        /// </summary>
        public static double RoundTripTimeInMillisecond { get; private set; }

        /// <summary>
        /// The time difference between local machine and the server that represented
        /// by DateTime tick (1 DateTimeTick = 100 nanosecond) 
        /// </summary>
        public static long OffsetDateTimeTicksWithServer { get; private set; }

        /// <summary>
        /// Get the DateTime ticks elapsed in the server machine after it initialized
        /// TimeSynchronizer.
        /// The result will be used in messages' timestamp, but for the TimeSync message,
        /// the result need not be adjusted.
        /// </summary>
        /// <param name="type">message type</param>
        /// <returns>Elapsed DateTime ticks in the first worker machine</returns>
        public static long GetSynchronizedTicksForMessage(MessageType type)
        {
            long offset = type == MessageType.TimeSync ? 0 : OffsetDateTimeTicksWithServer;

            return GetRelativeDateTimeTicks() + offset;
        }

        /// <summary>
        /// Sync time between local machine and the server
        /// </summary>
        /// <param name="communicator">communicator used for sending/receiving messages</param>
        public static void SyncTime(MessageCommunicatorBase communicator)
        {
            // we use the master worker as server.
            if (communicator.WorkerId == CommunicationEndPoint.MasterWorkerId)
            {
                long serverTicks = GetRelativeDateTimeTicks();

                // server side;
                for (int i = 0; i < communicator.TotalWorkerCount; ++i)
                {
                    if (i == CommunicationEndPoint.MasterWorkerId)
                    {
                        continue;
                    }

                    TimeSyncMessage message
                        = new TimeSyncMessage(communicator.WorkerId, i);

                    message.ServerTicks = serverTicks;
                    communicator.SendMessage(message);
                }
            }
            else
            {
                // client side.
                TimeSyncMessage message = (TimeSyncMessage)communicator.ReceiveMessage(MessageType.TimeSync);

                OffsetDateTimeTicksWithServer = message.ServerTicks - GetRelativeDateTimeTicks();
            }
        }

        private static long GetRelativeDateTimeTicks()
        {
            return ConvertStopwatchTicksToDateTimeTicks(watch.ElapsedTicks);
        }

        private static long ConvertStopwatchTicksToDateTimeTicks(long stopwatchTicks)
        {
            // one DateTime ticks is 100-nano second, so we multiply the time by 10^7
            return (long)((double)stopwatchTicks * 10000000 / Stopwatch.Frequency);
        }
    }
}
