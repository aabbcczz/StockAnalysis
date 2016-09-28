using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public sealed class BiddingTimeRange
    {
        /// <summary>
        /// Inclusive bidding start time
        /// </summary>
        public TimeSpan StartTime { get; private set; }

        /// <summary>
        /// Exclusive bidding end time
        /// </summary>
        public TimeSpan EndTime { get; private set; }

        /// <summary>
        /// 竞价方式
        /// </summary>
        public BiddingMethod Method { get; private set; }

        /// <summary>
        /// 是否接受撤单
        /// </summary>
        public bool IsOrderCancellationAcceptable { get; private set; }

        public BiddingTimeRange(TimeSpan startTime, TimeSpan endTime, BiddingMethod method, bool acceptOrderCancellation)
        {
            if (startTime >= endTime)
            {
                throw new ArgumentException("start time must be smaller than end time");
            }

            StartTime = startTime;
            EndTime = endTime;
            Method = method;
            IsOrderCancellationAcceptable = acceptOrderCancellation;
        }
    }
}
