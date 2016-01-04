using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTrading
{
    enum HistoryDataCategory : int
    {
        OrderHistory = 0,  // 历史委托
        SucceededOrderHistory = 1,  // 历史成交
        DeliveryList = 2, // 交割单
    }
}
