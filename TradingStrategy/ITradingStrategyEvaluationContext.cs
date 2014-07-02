using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface ITradingStrategyEvaluationContext
    {
        public double GetCurrentCapital();

        public IEnumerable<string> GetAllEquityCodes();

        public bool ExistsEquity(string code);

        public IEnumerable<Equity> GetEquityDetails(string code);
    }
}
