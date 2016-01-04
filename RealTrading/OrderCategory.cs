using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTrading
{
    enum OrderCategory : int
    {
        Buy = 0,  // 买入
        Sell = 1,  // 卖出
        FinancingBuy = 2, // 融资买入
        MarginSell = 3,  // 融券卖出
        BuySecurityForReturningSecurity = 4, // 买券还券
        SellSecurityForReturningMoney = 5, // 卖券还款
        ReturnSecurityDirectly = 6  // 现券还券
    }
}
