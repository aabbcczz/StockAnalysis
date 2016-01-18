using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    enum DataCategory : int
    {
        /// <summary>
        /// 资金
        /// </summary>
        Capital = 0,

        /// <summary>
        /// 股票
        /// </summary>
        Stock = 1,   

        /// <summary>
        /// 当日委托
        /// </summary>
        OrderSubmittedToday = 2,  
 
        /// <summary>
        /// 当日成交
        /// </summary>
        OrderSucceededToday = 3, 

        /// <summary>
        /// 可取消委托
        /// </summary>
        CancellableOrder = 4,  

        /// <summary>
        /// 股东代码
        /// </summary>
        ShareholderRegistryCode = 5, 

        /// <summary>
        /// 融资余额
        /// </summary>
        FinancingBalance = 6, 
 
        /// <summary>
        /// 融券余额
        /// </summary>
        MarginBalance = 7, 

        /// <summary>
        /// 可融证券
        /// </summary>
        MarginableSecurity = 8,  
    }
}
