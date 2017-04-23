using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTrading.Utility;
using StockAnalysis.Share;

namespace StockTradingConsole
{
    class StockBuyingStateMachine : StockTradingStateMachineBase
    {
        private StateMachine<BuyingState, StockTradingInput> _stateMachine;

        private NewStock _stock;
        private int _orderNo = TradingHelper.InvalidOrderNo; // order number returned by SendOrder

        public StockBuyingStateMachine(NewStock stock)
        {
            if (stock == null)
            {
                throw new ArgumentNullException();
            }

            Name = stock.Name;
            _stock = stock;

            StateTransition<BuyingState, StockTradingInput>[] transitions
                = new StateTransition<BuyingState, StockTradingInput>[]
                {
                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Initial,
                        BuyingState.TriedToBuyInCollectiveBiddingPhase,
                        TryToBuyInCollectiveBiddingPhase),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.TriedToBuyInCollectiveBiddingPhase,
                        BuyingState.Cancelling,
                        CancelOrder),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Cancelling,
                        BuyingState.NewInitial,
                        WaitForFinalOrderStatus),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Initial,
                        BuyingState.Final,
                        IsOpenPriceNotAcceptable),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.NewInitial,
                        BuyingState.Final,
                        IsOpenPriceNotAcceptable),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Initial,
                        BuyingState.Final,
                        IsCapitalNotEnough),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.NewInitial,
                        BuyingState.Final,
                        IsCapitalNotEnough),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Initial,
                        BuyingState.TriedToBuyInContinuousBiddingPhase,
                        TryToBuyInContinuousBiddingPhase),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.NewInitial,
                        BuyingState.TriedToBuyInContinuousBiddingPhase,
                        TryToBuyInContinuousBiddingPhase),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.TriedToBuyInContinuousBiddingPhase,
                        BuyingState.NewInitial,
                        WaitForFinalOrderStatus),

                };

            _stateMachine = new StateMachine<BuyingState, StockTradingInput>(transitions, BuyingState.Initial);
        }

        public override bool IsFinalState()
        {
            return _stateMachine.IsFinalState();
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
                float remainingCapital = _stock.TotalCapitalUsedToBuy - result.DealVolume * result.DealPrice;

                NewStock stock = new NewStock()
                {
                    ActualOpenPrice = input.Quote.TodayOpenPrice,
                    BuyPriceDownLimitInclusive = _stock.BuyPriceDownLimitInclusive,
                    BuyPriceUpLimitInclusive = _stock.BuyPriceUpLimitInclusive,
                    DateToBuy = _stock.DateToBuy,
                    Name = _stock.Name,
                    TotalCapitalUsedToBuy = remainingCapital
                };

                // replace current NewStock object with updated NewStock object
                _stock = stock;

                AppLogger.Default.DebugFormat("[{0}] Remaining capital {1:F2}", _stock.Name.RawCode, remainingCapital);
            }

            return true;
        }

        private bool CancelOrder(StockTradingInput input)
        {
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
            if (!input.Client.CancelOrder(_stock.Name.RawCode, _orderNo, out error))
            {
                AppLogger.Default.ErrorFormat("[{0}] Failed to cancel order {1}, Error: {2}", _stock.Name.RawCode, _orderNo, error);
                return false;
            }

            return true;
        }

        private bool GetOrderResult(StockTradingInput input, int orderNo, out QueryGeneralOrderResult result)
        {
            AppLogger.Default.DebugFormat("[{0}] Try to get order result for order {1}", _stock.Name.RawCode, orderNo);

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

        private bool GetOrderResultInCollectiveBiddingPhrase(StockTradingInput input, int orderNo, out QueryGeneralOrderResult result)
        {
            result = null;

            if (!AfterCollectiveBiddingPhaseAndBeforeContinuousBuiddingPhase(input.Time))
            {
                return false;
            }

            return GetOrderResult(input, orderNo, out result);
        }

        private bool TryToBuyInCollectiveBiddingPhase(StockTradingInput input)
        {
            if (!InCollectiveBiddingPhase(input.Time)
                || input.Quote.IsTradingStopped())
            {
                return false;
            }

            var quote = input.Quote;
            float collectiveBiddingPrice;
            int collectiveBiddingVolumeInHand;

            if (quote.TryGetCollectiveBiddingPriceAndVolumeInHand(out collectiveBiddingPrice, out collectiveBiddingVolumeInHand))
            {
                AppLogger.Default.DebugFormat("[{0}] Collective bidding price {1:F2} volume {2} hand", _stock.Name.RawCode, collectiveBiddingPrice, collectiveBiddingVolumeInHand);

                if (_stock.IsPriceAcceptable(collectiveBiddingPrice))
                {
                    float buyingPrice = (float)Math.Round(_stock.BuyPriceUpLimitInclusive, 2);
                    int buyableVolumeInHand = _stock.BuyableVolumeInHand(buyingPrice);

                    // has enough buyer/seller, so that our bid will not impact market
                    if (collectiveBiddingVolumeInHand >= buyableVolumeInHand)
                    {
                        // try best not to impact market
                        int buyingVolumeInHand = Math.Min(buyableVolumeInHand, collectiveBiddingVolumeInHand / 3);
                        if (buyableVolumeInHand > 0)
                        {
                            AppLogger.Default.DebugFormat("[{0}] Buying price {1:F2} volume {2} hand", _stock.Name.RawCode, buyingPrice, buyableVolumeInHand);

                            int orderNo = Buy(input.Client, buyingPrice, buyableVolumeInHand);

                            if (!TradingHelper.IsInvalidOrderNo(orderNo))
                            {
                                _orderNo = orderNo;
                                input.OrderStatusTracker.RegisterOrder(orderNo);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool IsOpenPriceNotAcceptable(StockTradingInput input)
        {
            if (!AfterCollectiveBiddingPhaseAndBeforeContinuousBuiddingPhase(input.Time)
                || input.Quote.IsTradingStopped())
            {
                return false;
            }

            var quote = input.Quote;

            float openPrice = quote.TodayOpenPrice;
            AppLogger.Default.DebugFormat("[{0}] Open price {1:F2}", _stock.Name.RawCode, openPrice);

            bool notAcceptable = !_stock.IsPriceAcceptable(openPrice);
            if (notAcceptable)
            {
                AppLogger.Default.InfoFormat(
                    "[{0}] Failed to buy because open price is out of range [{1:F2}..{2:F2}]",
                    _stock.Name.RawCode, 
                    _stock.BuyPriceDownLimitInclusive, 
                    _stock.BuyPriceUpLimitInclusive);
            }

            return notAcceptable;
        }

        private bool IsCapitalNotEnough(StockTradingInput input)
        {
            if (!InContinuousBiddingPhase(input.Time)
                || input.Quote.IsTradingStopped())
            {
                return false;
            }

            var quote = input.Quote;

            // capital is not enough for 1 hand for open price
            if (_stock.TotalCapitalUsedToBuy < quote.TodayOpenPrice * ChinaStockHelper.ConvertHandToVolume(1))
            {
                AppLogger.Default.InfoFormat(
                    "[{0}] Failed to buy because no enough capital {1:F2} for 1 hand for price {2:F2}",
                    _stock.Name.RawCode,
                    _stock.TotalCapitalUsedToBuy,
                    quote.TodayOpenPrice);

                return true;
            }

            return false;
        }

        private bool TryToBuyInContinuousBiddingPhase(StockTradingInput input)
        {
            if (!InContinuousBiddingPhase(input.Time)
                || input.Quote.IsTradingStopped())
            {
                return false;
            }

            var quote = input.Quote;

            float openPrice = quote.TodayOpenPrice;

            if (!_stock.IsPriceAcceptable(openPrice))
            {
                return false;
            }

            // 如果卖一价比开盘价高，说明买盘比较强，抬高0.1%的价格买，如果卖二价也低于开盘价，以卖二价买，否则以开盘价买
            float buyingPrice;

            if (quote.SellPrices[0] > openPrice)
            {
                buyingPrice = openPrice * 1.001f;
            }
            else if (TradingHelper.IsValidPrice(quote.SellPrices[1]) && quote.SellPrices[1] < openPrice)
            {
                buyingPrice = quote.SellPrices[1];
            }
            else
            {
                buyingPrice = openPrice;
            }

            buyingPrice = (float)Math.Round(Math.Min(quote.GetUpLimitPrice(), (double)buyingPrice), 2);

            int buyableVolumeInHand = _stock.BuyableVolumeInHand(buyingPrice);
            int applicableVolumeInHand = quote.EstimateApplicableVolumeInHandForBuyingPrice(buyingPrice);

            int buyingVolumeInHand = Math.Min(buyableVolumeInHand, applicableVolumeInHand);

            if (buyingVolumeInHand == 0)
            {
                return false;
            }

            AppLogger.Default.DebugFormat("[{0}] Buying price {1:F2} volume {2} hand", _stock.Name.RawCode, buyingPrice, buyingVolumeInHand);

            int orderNo = Buy(input.Client, buyingPrice, buyingVolumeInHand);

            if (!TradingHelper.IsInvalidOrderNo(orderNo))
            {
                _orderNo = orderNo;

                input.OrderStatusTracker.RegisterOrder(orderNo);

                return true;
            }
            else
            {
                return false;
            }

        }

        private bool InCollectiveBiddingPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 24, 30);
            TimeSpan end = new TimeSpan(9, 24, 50);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool InContinuousBiddingPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 29, 30);
            TimeSpan end = new TimeSpan(14, 50, 0);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool AfterCollectiveBiddingPhaseAndBeforeContinuousBuiddingPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 26, 00);
            TimeSpan end = new TimeSpan(9, 29, 00);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private int Buy(TradingClient client, float buyingPrice, int volumeInHand)
        {
            OrderRequest request = new OrderRequest(_stock)
            {
                Category = OrderCategory.Buy,
                Price = buyingPrice,
                PricingType = StockOrderPricingType.LimitPrice,
                Volume = ChinaStockHelper.ConvertHandToVolume(volumeInHand),
                SecurityCode = _stock.Name.RawCode,
                SecurityName = _stock.Name.Names[0]
            };

            AppLogger.Default.InfoFormat("[{0}] Prepare buy request: {1}", _stock.Name.RawCode, request);

            string error;
            var result = client.SendOrder(request, out error);

            if (result == null)
            {
                AppLogger.Default.ErrorFormat("[{0}] Failed to send request. error: {1}", _stock.Name.RawCode, error);
                return TradingHelper.InvalidOrderNo;
            }

            AppLogger.Default.InfoFormat(
                "[{0}] SendOrder result: OrderNo: {1}, ReturnedInfo: {2}, ReservedInfo {3}, CheckingRiskFlag: {4}",
                _stock.Name.RawCode,
                result.OrderNo, 
                result.ReturnedInfo, 
                result.ReservedInfo, 
                result.CheckingRiskFlag);

            return result.OrderNo;
        }

        public override void ProcessQuote(TradingClient client, OrderStatusTracker tracker, FiveLevelQuote quote, DateTime time)
        {
            AppLogger.Default.DebugFormat("[{0}] State: {1} Time:{2:u} Quote:{3}", _stock.Name.RawCode, _stateMachine.CurrentState, time, quote);

            var input = new StockTradingInput(client, tracker, quote, time);

            _stateMachine.ProcessInput(input);
        }
    }
}
