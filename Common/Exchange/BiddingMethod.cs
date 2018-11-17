namespace StockAnalysis.Common.Exchange
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
