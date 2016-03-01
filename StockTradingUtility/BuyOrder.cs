using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
namespace StockTrading.Utility
{
    public sealed class BuyOrder
    {
        /// <summary>
        /// 期待的购买价格
        /// </summary>
        public float ExpectedMaxPrice { get; private set; }

        /// <summary>
        /// 允许的最高报价
        /// </summary>
        public float MaxBidPrice { get; private set; }

        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityCode { get; private set; }

        /// <summary>
        /// 证券名称
        /// </summary>
        public string SecurityName { get; private set; }

        /// <summary>
        /// 所属交易所
        /// </summary>
        public Exchange Exchange { get; private set; }

        /// <summary>
        /// 已购买数量
        /// </summary>
        public int BoughtVolume { get; private set; }

        /// <summary>
        /// 还可以用于购买的资金量
        /// </summary>
        public float RemainingCapitalCanBeUsed { get; private set; }

        /// <summary>
        /// 还可购买的数量
        /// </summary>
        public int RemainingVolumeCanBeBought { get; private set; }

        public BuyOrder(BuyInstruction instruction)
        {
            ExpectedMaxPrice = instruction.ExpectedMaxPrice;
            MaxBidPrice = instruction.MaxBidPrice;
            SecurityName = instruction.SecurityName;
            SecurityCode = instruction.SecurityCode;
            Exchange = StockTrading.Utility.Exchange.GetTradeableExchangeForSecurity(SecurityCode);
            BoughtVolume = 0;
            RemainingCapitalCanBeUsed = instruction.MaxCapitalCanBeUsed;
            RemainingVolumeCanBeBought = instruction.MaxVolumeCanBeBought;
        }

        public void Fulfill(float averagePrice, int volume)
        {
            lock (this)
            {
                BoughtVolume += volume;
                RemainingVolumeCanBeBought -= volume;
                RemainingCapitalCanBeUsed -= averagePrice * volume;
            }
        }

        /// <summary>
        /// 得到还可以买的量, 以手为单位。
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public int GetMaxVolumeInHandToBuy(float estimatePrice)
        {
            int minVolume = Math.Max(
                0, 
                (int)Math.Min(
                    RemainingCapitalCanBeUsed / estimatePrice, 
                    (float)RemainingVolumeCanBeBought));

            return minVolume / ChinaStockHelper.VolumePerHand;
        }

        public bool IsFinished(float minPrice)
        {
            if (minPrice <= 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (RemainingCapitalCanBeUsed < minPrice * ChinaStockHelper.VolumePerHand
                || RemainingVolumeCanBeBought < ChinaStockHelper.VolumePerHand)
            {
                return true;
            }

            return false;
        }
    }
}
