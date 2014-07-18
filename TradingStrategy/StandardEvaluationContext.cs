using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingStrategy
{
    internal sealed class StandardEvaluationContext : IEvaluationContext
    {
        private long _instructionId = 0;
        private EquityManager _equityManager;
        private ILogger _logger;

        public StandardEvaluationContext(EquityManager manager, ILogger logger)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            _equityManager = manager;
            _logger = logger;
        }

        public long GetUniqueInstructionId()
        {
            return _instructionId++;
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

        public void Log(string log)
        {
            if (_logger != null)
            {
                _logger.Log(log);
            }
        }
    }
}
