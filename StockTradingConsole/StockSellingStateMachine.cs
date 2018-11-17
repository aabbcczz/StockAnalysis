using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTrading.Utility;
using StockAnalysis.Share;
using StockAnalysis.Common.Utility;
using StockAnalysis.Common.Exchange;

namespace StockTradingConsole
{
    class StockSellingStateMachine : StockTradingStateMachineBase
    {
        private StateMachine<SellingState, StockTradingInput> _stateMachine;

        private OldStock _stock;
        private int _orderNo = TradingHelper.InvalidOrderNo; // order number returned by SendOrder
        private float _todayLowestPrice = float.MaxValue;

        public StockSellingStateMachine(OldStock stock)
        {
            if (stock == null)
            {
                throw new ArgumentNullException();
            }

            Name = stock.Name;
            _stock = stock;

            /* 
                                       TryToBuyInCollectiveBiddingPhase                                                               
                 initial --------------------------------------------------------> TriedToBuyInCollectiveBiddingPhase 
                     |                                                                              | 
                     |  IsOpenPriceNotAcceptable/IsCapitalNotEnough                                 | 
                     |---------------> Final                                                        | CancelOrder
                     |                    ^                                                         | 
                     |                    |IsOpenPriceNotAcceptable/IsCapitalNotEnough              | 
                     |                    |                                                         V
                     |  |--------------NewInitial <-------------------------------------------  Cancelling
                     |  |                    ^                WaitForFinalOrderStatus
          TryToBuyInContinuousBiddingPhase   |
                     |  |                    |--------| 
                     V  V                             | WaitForFinalOrderStatus  
            TriedToBuyInContinuousBiddingPhase -------|

             */
            StateTransition<SellingState, StockTradingInput>[] transitions
                = new StateTransition<SellingState, StockTradingInput>[]
                {
                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.Initial,
                        SellingState.InStrictUpLimit,
                        IsStrictUpLimit),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.Initial,
                        SellingState.NotInStrictUpLimit,
                        IsNotStrictUpLimit),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.Initial,
                        SellingState.ReadyForSelling,
                        IsReadyForSelling),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.InStrictUpLimit,
                        SellingState.NotInStrictUpLimit,
                        IsNotStrictUpLimit),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.NotInStrictUpLimit,
                        SellingState.TriedToSellAtUpLimitPrice,
                        TryToSellAtUpLimitPrice),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.TriedToSellAtUpLimitPrice,
                        SellingState.Cancelling,
                        CancelUpLimitOrder),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.Cancelling,
                        SellingState.ReadyForSelling,
                        WaitForFinalOrderStatus),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.ReadyForSelling,
                        SellingState.Final,
                        IsNoEnoughForSelling),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.ReadyForSelling,
                        SellingState.Final,
                        TryToSellInCollectiveBiddingPhase),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.ReadyForSelling,
                        SellingState.SoldInContinuousBiddingPhase,
                        TryToSellInContinuousBiddingPhase),

                    new StateTransition<SellingState, StockTradingInput>(
                        SellingState.SoldInContinuousBiddingPhase,
                        SellingState.ReadyForSelling,
                        WaitForFinalOrderStatus),

                };

            _stateMachine = new StateMachine<SellingState, StockTradingInput>(transitions, SellingState.Initial);
        }

        public override bool IsFinalState()
        {
            return _stateMachine.IsFinalState();
        }

        public override void ProcessQuote(TradingClient client, OrderStatusTracker tracker, FiveLevelQuote quote, DateTime time)
        {
            AppLogger.Default.DebugFormat("[{0}] State: {1} Time:{2:u} Quote:{3}", _stock.Name, _stateMachine.CurrentState, time, quote);

            var input = new StockTradingInput(client, tracker, quote, time);

            _stateMachine.ProcessInput(input);
        }

        private bool IsNoEnoughForSelling(StockTradingInput input)
        {
            if (_stock == null || _stock.Volume < ChinaStockHelper.ConvertHandToVolume(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool InCollectiveBiddingPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(14, 57, 30);
            TimeSpan end = new TimeSpan(15, 0, 0);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool InTryToSellAtUpLimitPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 29, 00);
            TimeSpan end = new TimeSpan(14, 50, 0);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool InCancelUpLimitOrderPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(14, 50, 30);
            TimeSpan end = new TimeSpan(14, 56, 0);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool IsReadyForSelling(StockTradingInput input)
        {
            TimeSpan begin = new TimeSpan(14, 50, 30);
            TimeSpan end = new TimeSpan(14, 56, 30);

            return input.Time.TimeOfDay >= begin && input.Time.TimeOfDay <= end;
        }

        private bool IsStrictUpLimitInternal(StockTradingInput input)
        {
            FiveLevelQuote quote = input.Quote;

            _todayLowestPrice = Math.Min(_todayLowestPrice, quote.CurrentPrice);

            if (quote.TodayOpenPrice == quote.GetUpLimitPrice()
                && _todayLowestPrice == quote.TodayOpenPrice)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsStrictUpLimit(StockTradingInput input)
        {
            if (!InTryToSellAtUpLimitPhase(input.Time))
            {
                return false;
            }

            return IsStrictUpLimitInternal(input);
        }

        private bool IsNotStrictUpLimit(StockTradingInput input)
        {
            if (!InTryToSellAtUpLimitPhase(input.Time))
            {
                return false;
            }

            return !IsStrictUpLimitInternal(input);
        }

        private bool TryToSellAtUpLimitPrice(StockTradingInput input)
        {
            if (!InTryToSellAtUpLimitPhase(input.Time))
            {
                return false;
            }

            if (IsStrictUpLimitInternal(input))
            {
                return false;
            }

            int sellingVolumeInHand = ChinaStockHelper.ConvertVolumeToHand(_stock.Volume);
            if (sellingVolumeInHand == 0)
            {
                return false;
            }

            float sellingPrice = (float)input.Quote.GetUpLimitPrice();

            AppLogger.Default.DebugFormat("[{0}] Selling price {1:F2} volume {2} hand", _stock.Name, sellingPrice, sellingVolumeInHand);

            int orderNo = SellAtLimitPrice(input.Client, sellingPrice, sellingVolumeInHand);
            if (TradingHelper.IsInvalidOrderNo(orderNo))
            {
                _orderNo = orderNo;
                input.OrderStatusTracker.RegisterOrder(orderNo);

                return true;
            }

            return false;
        }

        private int SendOrderRequest(TradingClient client, OrderRequest request)
        {
            string error;
            var result = client.SendOrder(request, out error);

            if (result == null)
            {
                AppLogger.Default.ErrorFormat("[{0}] Failed to send request. error: {1}", _stock.Name, error);
                return TradingHelper.InvalidOrderNo;
            }

            AppLogger.Default.InfoFormat(
                "[{0}] SendOrder result: OrderNo: {1}, ReturnedInfo: {2}, ReservedInfo {3}, CheckingRiskFlag: {4}",
                _stock.Name,
                result.OrderNo,
                result.ReturnedInfo,
                result.ReservedInfo,
                result.CheckingRiskFlag);

            return result.OrderNo;
        }

        private int SellAtLimitPrice(TradingClient client, float sellingPrice, int volumeInHand)
        {
            OrderRequest request = new OrderRequest(_stock)
            {
                Category = OrderCategory.Sell,
                Price = sellingPrice,
                PricingType = OrderPricingType.LimitPrice,
                Volume = ChinaStockHelper.ConvertHandToVolume(volumeInHand),
                SecuritySymbol = _stock.Name.Symbol.RawSymbol,
                SecurityName = _stock.Name.Names[0]
            };

            AppLogger.Default.InfoFormat("[{0}] Prepare sell request: {1}", _stock.Name, request);

            return SendOrderRequest(client, request);
        }

        private int SellAtMarketPrice(TradingClient client, int volumeInHand)
        {
            OrderRequest request = new OrderRequest(_stock)
            {
                Category = OrderCategory.Sell,
                Price = 0.01f,
                PricingType = OrderPricingType.MarketPriceMakeDealInFiveGradesThenCancel,
                Volume = ChinaStockHelper.ConvertHandToVolume(volumeInHand),
                SecuritySymbol = _stock.Name.Symbol.RawSymbol,
                SecurityName = _stock.Name.Names[0]
            };

            AppLogger.Default.InfoFormat("[{0}] Prepare sell request: {1}", _stock.Name, request);

            return SendOrderRequest(client, request);
        }

        private bool CancelUpLimitOrder(StockTradingInput input)
        {
            if (!InCancelUpLimitOrderPhase(input.Time))
            {
                return false;
            }

            QueryGeneralOrderResult result;
            if (!GetOrderResult(input, _orderNo, out result))
            {
                return false;
            }

            if (TradingHelper.IsFinalStatus(result.Status))
            {
                return true;
            }

            string error;
            // try to cancel order
            if (!input.Client.CancelOrder(_stock.Name.Symbol.RawSymbol, _orderNo, out error))
            {
                AppLogger.Default.ErrorFormat("[{0}] Failed to cancel order {1}, Error: {2}", _stock.Name, _orderNo, error);
                return false;
            }

            return true;
        }

        private bool GetOrderResult(StockTradingInput input, int orderNo, out QueryGeneralOrderResult result)
        {
            AppLogger.Default.DebugFormat("[{0}] Try to get order result for order {1}", _stock.Name, orderNo);

            result = null;

            if (TradingHelper.IsInvalidOrderNo(_orderNo))
            {
                throw new InvalidOperationException("Invalid orderNo");
            }

            result = input.OrderStatusTracker.GetOrderStatus(_orderNo);
            if (result == null)
            {
                return false;
            }

            return true;
        }

        private bool WaitForFinalOrderStatus(StockTradingInput input)
        {
            QueryGeneralOrderResult result;
            if (!GetOrderResult(input, _orderNo, out result))
            {
                return false;
            }

            if (!TradingHelper.IsFinalStatus(result.Status))
            {
                return false;
            }

            input.OrderStatusTracker.UnregisterOrder(_orderNo);
            _orderNo = TradingHelper.InvalidOrderNo;

            if (result.DealVolume > 0)
            {
                int remainingVolume = _stock.Volume - result.DealVolume;

                OldStock stock = new OldStock()
                {
                    Volume = remainingVolume,
                    Name = _stock.Name,
                };

                // replace current OldStock object with updated OldStock object
                _stock = stock;

                AppLogger.Default.DebugFormat("[{0}] Remaining volume {1:F2}", _stock.Name, remainingVolume);
            }

            return true;
        }

        private bool TryToSellInCollectiveBiddingPhase(StockTradingInput input)
        {
            if (!InCollectiveBiddingPhase(input.Time))
            {
                return false;
            }

            if (_stock.Name.Symbol.ExchangeId != ExchangeId.ShenzhenSecurityExchange)
            {
                return false;
            }

            int sellingVolumeInHand = ChinaStockHelper.ConvertVolumeToHand(_stock.Volume);
            if (sellingVolumeInHand == 0)
            {
                return false;
            }

            float sellingPrice = (float)input.Quote.GetDownLimitPrice();

            AppLogger.Default.DebugFormat("[{0}] Selling price {1:F2} volume {2} hand", _stock.Name, sellingPrice, sellingVolumeInHand);

            int orderNo = SellAtLimitPrice(input.Client, sellingPrice, sellingVolumeInHand);
            if (TradingHelper.IsInvalidOrderNo(orderNo))
            {
                _orderNo = orderNo;
                input.OrderStatusTracker.RegisterOrder(orderNo);

                return true;
            }

            return false;
        }

        private bool TryToSellInContinuousBiddingPhase(StockTradingInput input)
        {
            if (!InCollectiveBiddingPhase(input.Time))
            {
                return false;
            }

            if (_stock.Name.Symbol.ExchangeId != ExchangeId.ShanghaiSecurityExchange)
            {
                return false;
            }

            int sellingVolumeInHand = ChinaStockHelper.ConvertVolumeToHand(_stock.Volume);
            if (sellingVolumeInHand == 0)
            {
                return false;
            }

            AppLogger.Default.DebugFormat("[{0}] Selling volume {1} hand", _stock.Name, sellingVolumeInHand);

            int orderNo = SellAtMarketPrice(input.Client, sellingVolumeInHand);
            if (TradingHelper.IsInvalidOrderNo(orderNo))
            {
                _orderNo = orderNo;
                input.OrderStatusTracker.RegisterOrder(orderNo);

                return true;
            }

            return false;
        }
    }
}
