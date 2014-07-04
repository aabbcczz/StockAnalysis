using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface ITradingStrategyEvaluationContext
    {
        double GetCurrentCapital();

        IEnumerable<string> GetAllEquityCodes();

        bool ExistsEquity(string code);

        IEnumerable<Equity> GetEquityDetails(string code);
    }
}
