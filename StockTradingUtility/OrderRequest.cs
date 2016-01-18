using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    sealed class OrderRequest
    {
        /// <summary>
        /// 委托类型
        /// </summary>
        public OrderCategory Category { get; set; }

        /// <summary>
        /// 委托价格类型
        /// </summary>
        public OrderPriceType PriceType { get; set; }

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
        public int Quantity { get; set; }
    }
}
