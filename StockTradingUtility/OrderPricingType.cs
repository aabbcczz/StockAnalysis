using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    public enum OrderPricingType : int
    {
        /// <summary>
        /// 限价委托
        /// </summary>
        LimitPrice = 0, 
 
        /// <summary>
        /// 深圳 对方最优价格市价委托
        /// </summary>
        SZMarketPriceBestForCounterparty = 1,  
 
        /// <summary>
        /// 深圳 本方最优价格市价委托
        /// </summary>
        SZMarketPriceBestForSelf = 2,   
 
        /// <summary>
        /// 深圳 即时成交剩余撤销市价委托
        /// </summary>
        SZMarketPriceMakeDealImmediatelyThenCancel = 3,

        /// <summary>
        /// 五档即成剩余撤销市价委托
        /// </summary>
        MakertPriceMakeDealInFiveGradesThenCancel = 4, 
 
        /// <summary>
        /// 深圳 全额成交或撤销市价委托
        /// </summary>
        SZMakertPriceFullOrCancel = 5,

        /// <summary>
        /// 上海 五档即成转限价市价委托
        /// </summary>
        SHMakertPriceMakeDealInFiveGradesThenTurnToLimitPrice = 6,
    }
}
