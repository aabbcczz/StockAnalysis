using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTradingConsole
{
    class OrderTracker
    {
        public enum OrderType
        {
            Buy = 0,
            Sell
        };

        // pending order ids;
        private List<int> _pendingOrderIds = new List<int>();

        // total capital for trading. if order type is "buy", this is the current captial can be used for trading
        // otherwise, this is current capital gotten by selling stock
        private float _currentCapital;

        // processed volume (in hand) for selling or buying.
        private int _processedVolumeInHand;

        private OrderType _orderType;

        public OrderType Type { get { return _orderType; } }

        public float CurrentCapital { get { return _currentCapital; } }

        public int ProcessedVolumeInHand { get { return _processedVolumeInHand; } }

        public OrderTracker(NewStock stock)
        {
            this._orderType = OrderType.Buy;
            this._processedVolumeInHand = 0;
            this._currentCapital = stock.TotalCapitalUsedToBuy;
        }
    }
}
