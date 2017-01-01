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
        enum InternalState
        {
            NotReady = 0, // 初始状态
            BuyInCollectiveBiddingPhase, // 在集合竞价时下单，09:24:30->09:24:50
            BuyAfterCollectiveBiddingAndBeforeContinuousBidding, // 在09:26:00->09:29:30间下单
            BuyInContinuousBiddingPhase, // 在连续竞价时下单 09:30:00->14:50:00
            PartiallyBought, // 部分买成
            FullyBought, // 全部买成
            FailedToBuy, // 购买失败
        };

        private InternalState _state = InternalState.NotReady;
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
        }

        public override bool IsFinalState()
        {
            return _state == InternalState.FailedToBuy;
        }

        private bool InCollectiveBiddingPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 24, 30);
            TimeSpan end = new TimeSpan(9, 24, 50);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool AfterCollectiveBiddingBeforeContinuousBidding(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 26, 00);
            TimeSpan end = new TimeSpan(9, 29, 30);

            return time.TimeOfDay >= begin && time.TimeOfDay <= end;
        }

        private bool InContinuousBiddingPhase(DateTime time)
        {
            TimeSpan begin = new TimeSpan(9, 30, 0);
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

        public override void HandleQuote(TradingClient client, FiveLevelQuote quote, DateTime time)
        {
            AppLogger.Default.DebugFormat("Time:{0:u} Quote:{1}", time, quote);

            switch (_state)
            {
                case InternalState.NotReady:
                    if (InCollectiveBiddingPhase(time))
                    {
                        if (quote.IsTradingStopped())
                        {
                            break;
                        }

                        float collectiveBiddingPrice;
                        int collectiveBiddingVolumeInHand;

                        if (quote.TryGetCollectiveBiddingPriceAndVolumeInHand(out collectiveBiddingPrice, out collectiveBiddingVolumeInHand))
                        {
                            AppLogger.Default.DebugFormat("Collective bidding price {0:F2} volume {1} hand", collectiveBiddingPrice, collectiveBiddingVolumeInHand);

                            if (_stock.IsBuyablePrice(collectiveBiddingPrice))
                            {
                                float buyingPrice = (float)Math.Round(Math.Min(collectiveBiddingPrice * 1.02, _stock.BuyPriceUpLimitInclusive), 2);
                                int buyableVolumeInHand = _stock.BuyableVolumeInHand(buyingPrice);

                                // has enough buyer/Seller, so that our bid will not impact market
                                if (collectiveBiddingVolumeInHand >= buyableVolumeInHand * 5)
                                {
                                    AppLogger.Default.DebugFormat("Buying price {0:F2} volume {1} hand", buyingPrice, buyableVolumeInHand);

                                    if (Buy(client, buyingPrice, buyableVolumeInHand))
                                    {
                                        _state = InternalState.BuyInCollectiveBiddingPhase;
                                    }
                                }
                            }
                        }

                        break;
                    }

                    if (AfterCollectiveBiddingBeforeContinuousBidding(time))
                    {
                        if (quote.IsTradingStopped())
                        {
                            break;
                        }

                        float openPrice = quote.TodayOpenPrice;
                        AppLogger.Default.DebugFormat("Open price {0:F2}", openPrice);

                        if (_stock.IsBuyablePrice(openPrice))
                        {
                            float buyingPrice = (float)Math.Round(Math.Min(openPrice * 1.02, _stock.BuyPriceUpLimitInclusive), 2);
                            int buyableVolumeInHand = _stock.BuyableVolumeInHand(buyingPrice);

                            // has enough buyer/Seller, so that our bid will not impact market
                            if (collectiveBiddingVolumeInHand >= buyableVolumeInHand * 5)
                            {
                                AppLogger.Default.DebugFormat("Buying price {0:F2} volume {1} hand", buyingPrice, buyableVolumeInHand);

                                if (Buy(client, buyingPrice, buyableVolumeInHand))
                                {
                                    _state = InternalState.BuyInCollectiveBiddingPhase;
                                }
                            }
                        }

                        break;
                    }

                    break;
                default:
                    throw new InvalidOperationException(string.Format("unsupported state {0}", _state));

            }
        }
    }
}
