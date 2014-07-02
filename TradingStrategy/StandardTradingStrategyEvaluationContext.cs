using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    internal sealed class StandardTradingStrategyEvaluationContext : ITradingStrategyEvaluationContext
    {
        private EquityManager _equityManager;

        public StandardTradingStrategyEvaluationContext(EquityManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            _equityManager = manager;
        }

        public double GetCurrentCapital()
        {
            return _equityManager.CurrentCapital;
        }

        public IEnumerable<string> GetAllEquityCodes()
        {
            return _equityManager.GetAllEquityCodes();
        }

        public bool ExistsEquity(string code)
        {
            return _equityManager.ExistsEquity(code);
        }

        public IEnumerable<Equity> GetEquityDetails(string code)
        {
            return _equityManager.GetEquityDetails(code);
        }
    }
}
