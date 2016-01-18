using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    enum OrderCategory : int
    {
        /// <summary>
        /// 买入
        /// </summary>
        Buy = 0,  
 
        /// <summary>
        /// 卖出
        /// </summary>
        Sell = 1,  
 
        /// <summary>
        /// 融资买入
        /// </summary>
        FinancingBuy = 2, 
 
        /// <summary>
        /// 融券卖出
        /// </summary>
        MarginSell = 3,  
 
        /// <summary>
        /// 买券还券
        /// </summary>
        BuySecurityForReturningSecurity = 4, 
 
        /// <summary>
        /// 卖券还款
        /// </summary>
        SellSecurityForReturningMoney = 5, 
 
        /// <summary>
        /// 现券还券
        /// </summary>
        ReturnSecurityDirectly = 6,   
    }
}
