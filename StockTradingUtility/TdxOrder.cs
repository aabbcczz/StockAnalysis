namespace StockAnalysis.StockTrading.Utility
{
    using Common.Exchange;
    using System;

    public sealed class TdxOrder
    {
        /// <summary>
        /// 委托编号
        /// </summary>
        public int OrderNo { get; set; }

        /// <summary>
        /// 委托时间
        /// </summary>
        public DateTime SubmissionTime { get; set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecuritySymbol { get; set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; set; }

        /// <summary>
        /// 买卖标志, true means buy, false means sell
        /// </summary>
        public bool IsBuy { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public float SubmissionPrice { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int SubmissionVolume { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public float DealPrice { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealVolume { get; set; }

        /// <summary>
        /// 委托方式
        /// </summary>
        public OrderCategory SubmissionOrderCategory { get; set; }

        /// <summary>
        /// 报价方式
        /// </summary>
        public OrderPricingType PricingType { get; set; }
    }
}
