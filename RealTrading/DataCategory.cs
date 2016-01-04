using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTrading
{
    enum DataCategory : int
    {
        Capital = 0,  // 资金
        Stock = 1,   // 股票
        OrderSentToday = 2,  // 当日委托
        SucceededOrderTody = 3, // 当日成交
        CancellableOrder = 4,  // 可取消委托
        ShareholderRegistryCode = 5, // 股东代码
        FinancingBalance = 6, // 融资余额
        MarginBalance = 7, // 融券余额
        MarginableSecuirty = 8, // 可融证券
    }
}
