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
    public abstract class OrderBase : IOrder
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
        /// 期待执行数量
        /// </summary>
        public int ExpectedVolume { get; protected set; }

        /// <summary>
        /// 已执行数量
        /// </summary>
        public int ExecutedVolume { get; protected set; }

        /// <summary>
        /// 平均执行价格
        /// </summary>
        public float AverageExecutedPrice { get; protected set; }

        /// <summary>
        /// 是否需要取消交易订单如果交易订单没有及时成功
        /// </summary>
        public bool ShouldCancelIfNotSucceeded { get; protected set; }


        /// <summary>
        /// 当Order被执行后需要执行的Action
        /// </summary>
        /// <remarks>OnOrderExecuted(IOrder order, float dealPrice, int dealVolume)</remarks>
        public Action<IOrder, float, int> OnOrderExecuted { get; private set; }

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
        /// deal the order fully or partially
        /// </summary>
        /// <param name="dealPrice">price of deal</param>
        /// <param name="dealVolume">volume of deal</param>
        public virtual void Deal(float dealPrice, int dealVolume)
        {
            if (dealVolume < 0 || dealPrice < 0.0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (dealVolume + ExecutedVolume > ExpectedVolume)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "deal volume({0}) + executed volume({1}) > expected volume({2}",
                        dealVolume,
                        ExecutedVolume,
                        ExpectedVolume));
            }

            AverageExecutedPrice = (dealPrice * dealVolume + AverageExecutedPrice * ExecutedVolume) / (dealVolume + ExecutedVolume);

            ExecutedVolume += dealVolume;
        }

        /// <summary>
        /// Determine if this order is fully completed and no additional process required.
        /// </summary>
        /// <returns>true if the order is fully completed, otherwise false.</returns>
        public abstract bool IsCompleted();

        protected OrderBase(string securityCode, string securityName, int volume, Action<IOrder, float, int> onOrderExecuted)
        {
            if (string.IsNullOrWhiteSpace(securityCode))
            {
                throw new ArgumentNullException();
            }

            if (volume <= 0)
            {
                throw new ArgumentException();
            }

            OrderId = Guid.NewGuid();
            SecurityCode = securityCode;
            SecurityName = securityName;
            Exchange = StockTrading.Utility.Exchange.GetTradeableExchangeForSecurity(SecurityCode);
            ExpectedVolume = volume;
            ExecutedVolume = 0;
            ShouldCancelIfNotSucceeded = false;
            OnOrderExecuted = onOrderExecuted;
        }

        public override string ToString()
        {
            return string.Format(
                "Id: {0}, {1}/{2} expected volume: {3}, executed volume: {4}, average executed price: {5:0.000}",
                OrderId,
                SecurityCode,
                SecurityName,
                ExpectedVolume,
                ExecutedVolume,
                AverageExecutedPrice);
        }
    }
}
