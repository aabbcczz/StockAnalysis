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
        public Guid OrderId { get; private set; }

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
        /// 期待的购买价格
        /// </summary>
        public float ExpectedPrice { get; private set; }

        /// <summary>
        /// 允许的最高报价
        /// </summary>
        public float MaxBidPrice { get; private set; }

        /// <summary>
        /// 初始数量
        /// </summary>
        public int OriginalVolume { get; private set; }

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
            OrderId = Guid.NewGuid();

            ExpectedPrice = instruction.ExpectedPrice;
            MaxBidPrice = instruction.MaxBidPrice;
            SecurityName = instruction.SecurityName;
            SecurityCode = instruction.SecurityCode;
            Exchange = StockTrading.Utility.Exchange.GetTradeableExchangeForSecurity(SecurityCode);
            BoughtVolume = 0;
            OriginalVolume = instruction.MaxVolumeCanBeBought;
            RemainingCapitalCanBeUsed = instruction.MaxCapitalCanBeUsed;
            RemainingVolumeCanBeBought = instruction.MaxVolumeCanBeBought;
        }

        public void Fulfill(float dealPrice, int dealVolume)
        {
            lock (this)
            {
                BoughtVolume += dealVolume;
                RemainingVolumeCanBeBought -= dealVolume;
                RemainingCapitalCanBeUsed -= dealPrice * dealVolume;
            }
        }

        /// <summary>
        /// 得到还可以买的量, 以手为单位。
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public int GetMaxVolumeInHandToBuy(float estimatedPrice)
        {
            int minVolume = Math.Max(
                0, 
                (int)Math.Min(
                    RemainingCapitalCanBeUsed / estimatedPrice, 
                    (float)RemainingVolumeCanBeBought));

            return minVolume / ChinaStockHelper.VolumePerHand;
        }

        public bool IsCompleted(float minPrice)
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
