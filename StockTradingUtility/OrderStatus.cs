using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.StockTrading.Utility
{
    public enum OrderStatus : int
    {
        /// <summary>
        /// 未知状态
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 未报. 起始状态.
        /// </summary>
        NotSubmitted,  

        /// <summary>
        /// 废单
        /// </summary>
        InvalidOrder,

        /// <summary>
        /// 撤废, 撤单的废单, 极其罕见
        /// </summary>
        InvalidCancellation,

        /// <summary>
        /// 待撤
        /// </summary>
        PendingForCancellation,

        /// <summary>
        /// 正撤
        /// </summary>
        Cancelling,

        /// <summary>
        /// 部撤
        /// </summary>
        PartiallySucceededAndThenCancelled,

        /// <summary>
        /// 已撤
        /// </summary>
        Cancelled,

        /// <summary>
        /// 待报
        /// </summary>
        PendingForSubmission,

        /// <summary>
        /// 正报
        /// </summary>
        Submitting,

        /// <summary>
        /// 已报
        /// </summary>
        Submitted,

        /// <summary>
        /// 部成
        /// </summary>
        PartiallySucceeded,

        /// <summary>
        /// 已成
        /// </summary>
        CompletelySucceeded,
    }
}
