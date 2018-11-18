namespace StockAnalysis.StockTrading.Utility
{
    using System;
    using Common.Exchange;

    public sealed class OrderRequest
    {
        /// <summary>
        /// 客户端请求编号, 由构造函数自动生成。
        /// </summary>
        public Guid RequestId { get; private set; }

        /// <summary>
        /// 委托类型
        /// </summary>
        public OrderCategory Category { get; set; }

        /// <summary>
        /// 委托价格类型
        /// </summary>
        public OrderPricingType PricingType { get; set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecuritySymbol { get; set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; set; }

        /// <summary>
        /// 委托价格
        /// </summary>
        public float Price { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 关联对象, 由创建者指定
        /// </summary>
        public object AssociatedObject { get; private set; }

        public OrderRequest(object associatedObject)
        {
            RequestId = Guid.NewGuid();

            AssociatedObject = associatedObject;
        }

        public override string ToString()
        {
            return string.Format(
                "{0}/{1}, RequestId: {2}, Category: {3}, PricingType: {4}, Price: {5:0.000}, Volume: {6}",
                SecuritySymbol,
                SecurityName,
                RequestId,
                Category,
                PricingType,
                Price,
                Volume);
        }
    }
}
