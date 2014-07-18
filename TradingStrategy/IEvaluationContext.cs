using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    public interface IEvaluationContext
    {
        long GetUniqueInstructionId();

        double GetCurrentCapital();

        IEnumerable<string> GetAllEquityCodes();

        bool ExistsEquity(string code);

        IEnumerable<Equity> GetEquityDetails(string code);

        void Log(string log);
    }
}
