namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class MessageAggregator
    {
        private int _totalCount;
        private int _aggregatedCount;

        public MessageAggregator(int count) 
        { 
            _totalCount = count; 
        }

        public int Count
        {
            get { return Interlocked.CompareExchange(ref _aggregatedCount, 0, 0); }
        }

        public bool IsAggregatorFull()
        {
            return Count >= _totalCount;
        }

        public bool IsAggregatorEmpty()
        {
            return Count == 0;
        }

        public virtual void Reset() 
        { 
            _aggregatedCount = 0; 
        }

        public void AggregateOnce()
        {
            if (Interlocked.Increment(ref _aggregatedCount) > _totalCount)
            {
                throw new InvalidOperationException("number of aggregation exceeds expects");
            }
        }
    }
}
