using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockAnalysis.Share;
using TradingStrategy;

namespace DataStatistic
{
    interface IBarCounter
    {
        string Name { get; }

        void Count(Bar[] bars, ITradingObject tradingObject);

        void SaveResults(string outputFile);
    }
}
