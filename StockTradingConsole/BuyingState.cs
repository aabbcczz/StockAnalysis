using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradingConsole
{
    enum BuyingState
    {
        Initial = 0, // 初始状态
        TriedToBuyInCollectiveBiddingPhase, // 在集合竞价时下单，09:24:30->09:24:50
        TriedToBuyInContinuousBiddingPhase, // 在连续竞价时下单 09:29:30->14:50:00
        Cancelling,  // 取消中
        NewInitial,  // inital status that only can be processed in continuous bidding phase
        Final, // 购买失败或者部分购买成功，并且无法继续
    };
}
