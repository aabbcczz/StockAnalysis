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

        public IEnumerable<string> GetAllPositionCodes()
        {
            return _equityManager.GetAllEquityCodes();
        }

        public bool ExistsPosition(string code)
        {
            return _equityManager.ExistsPosition(code);
        }

        public IEnumerable<Position> GetPositionDetails(string code)
        {
            return _equityManager.GetPositionDetails(code);
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
