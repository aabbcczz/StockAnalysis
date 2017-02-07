using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTrading.Utility;

namespace StockTradingConsole
{
    class StockSellingStateMachine : StockTradingStateMachineBase
    {
        enum InternalState
        {
            NotReady = 0, // 初始状态
            BuyInCollectiveBiddingPhase, // 集合竞价购买状态，09:24:30->09:24:50
            TimePast0926,
            Final,

        };

        private InternalState _state = InternalState.NotReady;

        public StockSellingStateMachine(OldStock stock)
        {
            Name = stock.Name;
        }

        public override bool IsFinalState()
        {
            return _state == InternalState.Final;
        }

        public override void ProcessQuote(TradingClient client, FiveLevelQuote quote, DateTime time)
        {
            return;
        }
    }
}
