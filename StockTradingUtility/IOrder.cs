using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrading.Utility
{
    /// <summary>
    /// Interface of Order
    /// </summary>
    public interface IOrder
    {
        /// <summary>
        /// Order unique id
        /// </summary>
        Guid OrderId { get; }

        /// <summary>
        /// 证券代码
        /// </summary>
        string SecurityCode { get; }

        /// <summary>
        /// 证券名称
        /// </summary>
        string SecurityName { get; }

        /// <summary>
        /// 所属交易所
        /// </summary>
        Exchange Exchange { get; }

        /// <summary>
        /// 期待执行数量
        /// </summary>
        int ExpectedVolume { get; }

        /// <summary>
        /// 已执行数量
        /// </summary>
        int ExecutedVolume { get; }

        /// <summary>
        /// Decide if this order should be executed based on given quote
        /// </summary>
        /// <param name="quote">quote of stock</param>
        /// <returns></returns>
        bool ShouldExecute(FiveLevelQuote quote);

        /// <summary>
        /// Build OrderRequest based on quote
        /// </summary>
        /// <param name="quote">quote of stock</param>
        /// <returns>OrderRequest object that will be sent to Exchange</returns>
        OrderRequest BuildRequest(FiveLevelQuote quote);

        /// <summary>
        /// Fulfill deal
        /// </summary>
        /// <param name="dealPrice">price of deal</param>
        /// <param name="dealVolume">volume of deal</param>
        void Fulfill(float dealPrice, int dealVolume);

        /// <summary>
        /// Determine if this order is fully completed and no additional process required.
        /// </summary>
        /// <returns>true if the order is fully completed, otherwise false.</returns>
        bool IsCompleted();
    }
}
