using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
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
        public string SecurityCode { get; set; }

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
    }
}
