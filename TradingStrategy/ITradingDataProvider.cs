using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;

namespace TradingStrategy
{
    public interface ITradingDataProvider
    {
        IEnumerable<DateTime> GetAllPeriods();

        IEnumerable<ITradingObject> GetAllTradingObjects();

        IEnumerable<Bar> GetWarmUpData(string code);

        /// <summary>
        /// Get bars for all trading objects in next period
        /// </summary>
        /// <param name="time">
        /// Output parameter to store the time of period
        /// </param>
        /// <returns>
        /// All bars in an array, and the size of array equals to number of trading objects returned by
        /// GetAllTradingObjects(), and the first bar is the data for the first trading object, the second bar
        /// is the data for the second trading object, and so on. if there is no data for a trading object 
        /// in the period, the bar returned will be invalid, call bar.Invalid() will return true.
        /// return null if there is no more period. 
        /// </returns>
        Bar[] GetNextPeriodData(out DateTime time);

        /// <summary>
        /// Get the last effective data before or equal given period for a given trading object that identified by code
        /// </summary>
        /// <param name="code">the code of trading object</param>
        /// <param name="period">time of period that the data should not exceed</param>
        /// <param name="bar">[OUT] returned data if any</param>
        /// <returns>true if there is data for the trading object, otherwise false is returned</returns>
        bool GetLastEffectiveData(string code, DateTime period, out Bar bar);
    }
}
