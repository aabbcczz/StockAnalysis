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
        BuyInCollectiveBiddingPhase, // 在集合竞价时下单，09:24:30->09:24:50
        BuyInContinuousBiddingPhase, // 在连续竞价时下单 09:29:30->14:50:00
        PartiallyBought, // 部分买成
        FullyBought, // 全部买成
        FailedToBuy, // 购买失败
    };
}
