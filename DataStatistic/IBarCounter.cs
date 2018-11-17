using TradingStrategy;
using StockAnalysis.Common.Data;

namespace DataStatistic
{
    interface IBarCounter
    {
        string Name { get; }

        void Count(Bar[] bars, ITradingObject tradingObject);

        void SaveResults(string outputFile);
    }
}
