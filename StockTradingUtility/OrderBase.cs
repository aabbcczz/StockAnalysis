using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    /// <summary>
    /// base class of all kinds of order
    /// </summary>
    public abstract class OrderBase
    {
        /// <summary>
        /// Order unique id
        /// </summary>
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
        /// 初始数量
        /// </summary>
        public int OriginalVolume { get; protected set; }

        /// <summary>
        /// 已执行数量
        /// </summary>
        public int ExecutedVolume { get; protected set; }

        /// <summary>
        /// Decide if this order should be executed based on given quote
        /// </summary>
        /// <param name="quote">quote of stock</param>
        /// <returns></returns>
        public abstract bool ShouldExecute(FiveLevelQuote quote);

        /// <summary>
        /// Build OrderRequest based on quote
        /// </summary>
        /// <param name="quote">quote of stock</param>
        /// <returns>OrderRequest object that will be sent to Exchange</returns>
        public abstract OrderRequest BuildRequest(FiveLevelQuote quote);

        /// <summary>
        /// Fulfill deal
        /// </summary>
        /// <param name="dealPrice">price of deal</param>
        /// <param name="dealVolume">volume of deal</param>
        public abstract void Fulfill(float dealPrice, int dealVolume);

        /// <summary>
        /// Determine if this order is fully completed and no additional process required.
        /// </summary>
        /// <returns>true if the order is fully completed, otherwise false.</returns>
        public abstract bool IsCompleted();

        protected OrderBase(string securityCode, string securityName, int volume)
        {
            if (string.IsNullOrWhiteSpace(securityCode))
            {
                throw new ArgumentNullException();
            }

            OrderId = Guid.NewGuid();
            SecurityCode = securityCode;
            SecurityName = securityName;
            Exchange = StockTrading.Utility.Exchange.GetTradeableExchangeForSecurity(SecurityCode);
            OriginalVolume = volume;
            ExecutedVolume = 0;
        }
    }
}
