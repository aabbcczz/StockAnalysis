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
        public IEnumerable<ITradingObject> GetAllTradingObjects();

        public IEnumerable<Bar> GetWarmUpData(string code);

        /// <summary>
        /// Get valid bars for all trading objects in next period
        /// </summary>
        /// <param name="time">
        /// Output parameter to store the time of period
        /// </param>
        /// <returns>
        /// All valid bars. 
        /// return null if there is no more period. 
        /// return empty dictionary if there is no data for next period.
        /// </returns>
        public IDictionary<string, Bar> GetNextPeriodData(out DateTime time);
    }
}
