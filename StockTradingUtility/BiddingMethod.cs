using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public enum BiddingMethod
    {
        /// <summary>
        /// 只接受委托, 不竞价
        /// </summary>
        NotBidding,

        /// <summary>
        /// 集合竞价
        /// </summary>
        CollectiveBidding,

        /// <summary>
        /// 连续竞价
        /// </summary>
        ContinuousBidding,
    }
}
