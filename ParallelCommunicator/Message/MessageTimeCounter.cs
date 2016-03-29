namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Performance counter for message latency. Because the whole FastRank does not used
    /// AutoPilot Performance counter and windows performance counter, we have to build our
    /// own performance counter.
    /// </summary>
    internal sealed class MessageTimeCounter
    {
        /// <summary>
        /// Maximum size of sample stored in the counter. when more samples are added, 
        /// The oldest sample will be removed.
        /// </summary>
        public const int MaxSampleSize = 10240;

        private int _sampleIndex = -1;
        private int _sampleSize = 0;
        private long[] _samples = new long[MessageTimeCounter.MaxSampleSize];

        /// <summary>
        /// Number of samples
        /// </summary>
        public long Count
        {
            get { return _sampleSize; }
        }

        /// <summary>
        /// Get the maximum message latency in current available samples.
        /// </summary>
        /// <returns>the maximum message latency in tick</returns>
        public long GetMaxTicks()
        {
            long max = long.MinValue;
            for (int i = 0; i < _sampleSize; i++)
            {
                if (_samples[i] > max)
                {
                    max = _samples[i];
                }
            }

            return max;
        }

        /// <summary>
        /// Get the minimum message latency in current available samples.
        /// </summary>
        /// <returns>the minimum message latency in tick</returns>
        public long GetMinTicks()
        {
            long min = long.MaxValue;
            for (int i = 0; i < _sampleSize; i++)
            {
                if (_samples[i] < min)
                {
                    min = _samples[i];
                }
            }

            return min;
        }

        /// <summary>
        /// Get the total message latency in current available samples.
        /// </summary>
        /// <returns>the total message latency in tick</returns>
        public long GetTotalTicks()
        {
            long sum = 0;

            for (int i = 0; i < _sampleSize; i++)
            {
                sum += _samples[i];
            }

            return sum;
        }

        /// <summary>
        /// Add a sample to the performance counter
        /// </summary>
        /// <param name="ticks">message latency in ticks</param>
        public void AddSample(long ticks)
        {
            // the array of samples are used in rotate.
            int index = Interlocked.Increment(ref _sampleIndex);
            index = (int)(((uint)index) % _samples.Length);

            _samples[index] = ticks;

            while (true)
            {
                int oldSampleSize = _sampleSize;
                if (oldSampleSize >= _samples.Length)
                {
                    break;
                }

                if (Interlocked.CompareExchange(ref _sampleSize, oldSampleSize + 1, oldSampleSize)
                    == oldSampleSize)
                {
                    break;
                }
            }
        }
    }
}
