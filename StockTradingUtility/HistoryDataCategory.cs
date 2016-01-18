using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    enum HistoryDataCategory : int
    {
        /// <summary>
        /// 历史委托
        /// </summary>
        OrderSubmittedInHistory = 0,  
 
        /// <summary>
        /// 历史成交
        /// </summary>
        OrderSucceededInHistory = 1,  
 
        /// <summary>
        /// 交割单
        /// </summary>
        DeliveryList = 2,  
    }
}
