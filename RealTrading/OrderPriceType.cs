using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTrading
{
    enum OrderPriceType : int
    {
        //0上海限价委托 深圳限价委托 1(市价委托)深圳对方最优价格  2(市价委托)深圳本方最优价格  3(市价委托)深圳即时成交剩余撤销  4(市价委托)上海五档即成剩撤 深圳五档即成剩撤 5(市价委托)深圳全额成交或撤销 6(市价委托)上海五档即成转限价
        LimitPrice = 0,  // 限价委托
        SZMarketPriceBestForCounterparty = 1,  // 深圳： 对方最优价格市价委托
        SZMarketPriceBestForSelf = 2,    // 深圳： 本方最优价格市价委托
        SZMarketPriceMakeDealImmediatelyThenCancel = 3, // 深圳： 即时成交剩余撤销市价委托
        MakertPriceMakeDealInFiveGradesThenCancel = 4, // 五档即成剩余撤销市价委托
        SZMakertPriceFullOrCancel = 5,  // 深圳：全额成交或撤销市价委托
        SHMakertPriceMakeDealInFiveGradesThenTurnToLimitPrice = 6  // 上海：五档即成转限价市价委托
    }
}
