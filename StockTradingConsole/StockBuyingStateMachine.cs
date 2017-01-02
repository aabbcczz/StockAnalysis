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
        private int _orderNo; // order number returned by SendOrder

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
                        BuyingState.BuyInCollectiveBiddingPhase,
                        TryBuyInCollectiveBiddingPhase),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Initial,
                        BuyingState.FailedToBuy,
                        IsOpenPriceNotBuyable),

                    new StateTransition<BuyingState, StockTradingInput>(
                        BuyingState.Initial,
                        BuyingState.BuyInContinuousBiddingPhase,
                        TryBuyInContinuousBiddingPhase),
                };

            _stateMachine = new StateMachine<BuyingState, StockTradingInput>(transitions, BuyingState.Initial);
        }

        public override bool IsFinalState()
        {
            return _stateMachine.IsFinalState();
        }

        private bool TryBuyInCollectiveBiddingPhase(StockTradingInput input)
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
                AppLogger.Default.DebugFormat("Collective bidding price {0:F2} volume {1} hand", collectiveBiddingPrice, collectiveBiddingVolumeInHand);

                if (_stock.IsBuyablePrice(collectiveBiddingPrice))
                {
                    float buyingPrice = (float)Math.Round(Math.Min(collectiveBiddingPrice * 1.02, _stock.BuyPriceUpLimitInclusive), 2);
                    int buyableVolumeInHand = _stock.BuyableVolumeInHand(buyingPrice);

                    // has enough buyer/seller, so that our bid will not impact market
                    if (collectiveBiddingVolumeInHand >= buyableVolumeInHand * 5)
                    {
                        AppLogger.Default.DebugFormat("Buying price {0:F2} volume(hand) {1}", buyingPrice, buyableVolumeInHand);

                        if (Buy(input.Client, buyingPrice, buyableVolumeInHand))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsOpenPriceNotBuyable(StockTradingInput input)
        {
            if (!InContinuousBiddingPhase(input.Time)
                || input.Quote.IsTradingStopped())
            {
                return false;
            }

            var quote = input.Quote;

            float openPrice = quote.TodayOpenPrice;
            AppLogger.Default.DebugFormat("Open price {0:F2}", openPrice);

            bool isBuyable = _stock.IsBuyablePrice(openPrice);
            if (!isBuyable)
            {
                AppLogger.Default.InfoFormat(
                    "Failed to buy {0} because open price is out of range [{1:F2}..{2:F2}]", 
                    quote.SecurityCode, 
                    _stock.BuyPriceDownLimitInclusive, 
                    _stock.BuyPriceUpLimitInclusive);
            }

            return !isBuyable;
        }

        private bool TryBuyInContinuousBiddingPhase(StockTradingInput input)
        {
            if (!InContinuousBiddingPhase(input.Time)
                || input.Quote.IsTradingStopped())
            {
                return false;
            }

            var quote = input.Quote;

            float openPrice = quote.TodayOpenPrice;
            AppLogger.Default.DebugFormat("Open price {0:F2}", openPrice);

            if (_stock.IsBuyablePrice(openPrice))
            {
                float buyingPrice = _stock.BuyPriceUpLimitInclusive;
                int buyableVolumeInHand = _stock.BuyableVolumeInHand(buyingPrice);
                int applicableVolumeInHand = quote.EstimateApplicableVolumeInHandForBuyingPrice(buyingPrice);

                float actualBuyingPrice = 0.0f;
                int actualBuyingVolumeInHand = 0;

                if (applicableVolumeInHand >= buyableVolumeInHand * 3)
                {
                    // has much more seller
                    actualBuyingPrice = buyingPrice;
                    actualBuyingVolumeInHand = buyableVolumeInHand;
                }
                else if (applicableVolumeInHand >= buyableVolumeInHand)
                {
                    // seller is enough, but 
                }

                // has enough buyer/Seller, so that our bid will not impact market
                if (quote.EstimateApplicableVolumeInHandForBuyingPrice(buyingPrice) >= buyableVolumeInHand)
                {
                    AppLogger.Default.DebugFormat("Buying price {0:F2} volume {1} hand", buyingPrice, buyableVolumeInHand);

                    if (Buy(input.Client, buyingPrice, buyableVolumeInHand))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        private bool Buy(TradingClient client, float buyingPrice, int volumeInHand)
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

            AppLogger.Default.InfoFormat("Prepare buy request: {0}", request);

            string error;
            var result = client.SendOrder(request, out error);

            if (result == null)
            {
                AppLogger.Default.ErrorFormat("Failed to send request. error: {0}", error);
                return false;
            }

            AppLogger.Default.InfoFormat(
                "SendOrder result: OrderNo: {0}, ReturnedInfo: {1}, ReservedInfo {2}, CheckingRiskFlag: {3}", 
                result.OrderNo, 
                result.ReturnedInfo, 
                result.ReservedInfo, 
                result.CheckingRiskFlag);

            _orderNo = result.OrderNo;
            return true;
        }

        public override void ProcessQuote(TradingClient client, FiveLevelQuote quote, DateTime time)
        {
            AppLogger.Default.DebugFormat("State: {2} Time:{0:u} Quote:{1}", time, quote, _stateMachine.CurrentState);

            var input = new StockTradingInput(client, quote, time);

            _stateMachine.ProcessInput(input);
        }
    }
}
