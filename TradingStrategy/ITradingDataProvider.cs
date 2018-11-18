using System;
using StockAnalysis.Common.Data;

namespace StockAnalysis.TradingStrategy
{
    public interface ITradingDataProvider
    {
        DateTime[] GetAllPeriodsOrdered();

        ITradingObject[] GetAllTradingObjects();

        /// <summary>
        /// Get index of trading object
        /// </summary>
        /// <param name="symbol">symbol of trading object</param>
        /// <returns>index of trading object. if the trading object does not exist, -1 will be returned</returns>
        int GetIndexOfTradingObject(string symbol);

        /// <summary>
        /// Get the first non-warmup data periods for all trading objects. Data time equals or exceeds the limit should be 
        /// treated as trading data.
        /// </summary>
        /// <returns></returns>
        DateTime[] GetFirstNonWarmupDataPeriods();

        /// <summary>
        /// Get bars for all trading objects for given period
        /// </summary>
        /// <returns>
        /// All bars in an array, and the size of array equals to number of trading objects returned by
        /// GetAllTradingObjects(), and the first bar is the data for the first trading object, the second bar
        /// is the data for the second trading object, and so on. if there is no data for a trading object 
        /// in the period, the bar returned will be invalid, call bar.Invalid() will return true.
        /// return null if there is no more period. 
        /// </returns>
        Bar[] GetDataOfPeriod(DateTime period);

        /// <summary>
        /// Get the last effective bar before or equal given period for a given trading object that identified by symbol
        /// </summary>
        /// <param name="index">the index of trading object</param>
        /// <param name="period">time of period that the data should not exceed</param>
        /// <param name="bar">[OUT] returned data if any</param>
        /// <returns>true if there is data for the trading object, otherwise false is returned</returns>
        bool GetLastEffectiveBar(int index, DateTime period, out Bar bar);

        /// <summary>
        /// Get all bar data for given trading object.
        /// </summary>
        /// <param name="index">index of trading object</param>
        /// <returns>all bar data ordered by time ascending. if there is not data exists, null is returned</returns>
        Bar[] GetAllBarsForTradingObject(int index);
    }
}
